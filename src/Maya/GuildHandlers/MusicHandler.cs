using Discord;
using Discord.WebSocket;
using Maya.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Maya.GuildHandlers
{
    public class MusicHandler
    {
        private GuildHandler GuildHandler;
        private IVoiceChannel voiceChannel;
        private MusicQueue queue;
        private MusicPlayer player;
        private readonly int MaxQueue = 10;
        public MusicHandler(GuildHandler GuildHandler)
        {
            this.GuildHandler = GuildHandler;
            voiceChannel = null;
            queue = new MusicQueue(this);
            player = new MusicPlayer(GuildHandler, this);
        }

        public void Initialize()
        {
        }

        public bool isReady()
        {
            if (queue == null)
                return false;
            if (player == null)
                return false;
            return true;
        }

        public MusicResult canUserUseMusicCommands(MusicContext context)
        {
            if (!(context.AskedBy is IGuildUser))
                return new MusicResult("null");
            var wrapper = GuildHandler.ConfigHandler.getMusic();
            if (!wrapper.isEnabled)
                return new MusicResult("null");
            if(!Utils.isChannelListed(context.Channel, wrapper.TextChannels))
                return new MusicResult("null");
            return new MusicResult();
        }

        public MusicResult canUserAddToQueue(MusicContext context, bool checkCmds = true)
        {
            if (checkCmds)
            {
                MusicResult mr = canUserUseMusicCommands(context);
                if (!mr.IsSuccessful)
                    return mr;
            }
            if ((context.AskedBy as IGuildUser).VoiceChannel != voiceChannel)
                return new MusicResult($"You need to be in the same music **voice** channel as the bot ({voiceChannel.Name}).");
            return new MusicResult();
        }

        public MusicQueue getMusicQueue()
        {
            return queue;
        }

        public MusicContext getCurrentSong()
        {
            return player.getCurrentSong();
        }

        public IVoiceChannel getVoiceChannel()
        {
            return voiceChannel;
        }

        public void Skip()
        {
            player.Skip();
        }

        public void Stop()
        {
            queue.Clear();
            Skip();
        }

        public async Task JoinVoiceChannel(IVoiceChannel channel)
        {
            if (player.audioClient != null)
                await player.audioClient.DisconnectAsync();
            voiceChannel = channel;
            player.audioClient = await channel.ConnectAsync();
        }

        public async Task Reset()
        {
            //TODO: Clear voice buffer
            Stop();
            await Task.Delay(1000);
            if (player.audioClient != null)
                await JoinVoiceChannel(voiceChannel);
            await GuildHandler.MainHandler.Client.SetGameAsync(null);
            player = new MusicPlayer(GuildHandler, this);
        }

        public void ChangeVolume(int n) //[0,100]
        {
            player.ChangeVolume(n / 100.0f);
        }

        public async Task Search(MusicContext music, string search)
        {
            music = new MusicSearch(music, search).Run();
            if (music == null)
            {
                await music.Channel.SendMessageAsync("No result found.");
                return;
            }
            if (isSongInPlaylist(music))
            {
                await music.Channel.SendMessageAsync($"Couldn't queue your request of **{music.Song.Title ?? music.Song.VideoId}** because it's already in the playlist.");
                return;
            }
            music = await new MusicPreemptiveDownload(music).Run();
            if (music == null)
            {
                await music.Channel.SendMessageAsync("Video not found.");
                return;
            }
            if (music.Song.Duration.GetValueOrDefault().TotalMinutes > 9)
            {
                await music.Channel.SendMessageAsync("Video length exceeds limit (more than 9 minutes).");
                return;
            }
            await Queue(music);
        }

        public async Task Queue(MusicContext music)
        {
            if (queue.getQueue().Count >= MaxQueue)
            {
                await music.Channel.SendMessageAsync($"Couldn't queue your request of **{music.Song.Title}** because the limit was reached. (Max: {MaxQueue})");
                return;
            }
            if (isSongInPlaylist(music))
            {
                await music.Channel.SendMessageAsync($"Couldn't queue your request of **{music.Song.Title ?? music.Song.VideoId}** because it's already in the playlist.");
                return;
            }
            await queue.Enqueue(music);
            await music.Channel.SendMessageAsync($"Enqueued **{music.Song.Title}** to the playlist.\nPosition: {queue.getQueue().Count} - estimated time until playing: {estimateTimeUntilLast(music)}");
            await Play();
        }

        public async Task Play()
        {
            await player.Run();
        }

        public string estimateTimeUntilLast(MusicContext until)
        {
            int secs = 0;
            if (player.getCurrentSong() != null)
                secs += player.getCurrentSong().Song.timeUntilEnd();
            foreach (MusicContext m in queue.getQueue())
            {
                if (m == until)
                    break;
                secs += (int)m.Song.Duration?.TotalSeconds;
            }
            TimeSpan ts = new TimeSpan(0, 0, secs);
            return $"{ts.Hours}:{(ts.Minutes < 10 ? $"0{ts.Minutes}" : $"{ts.Minutes}")}:{(ts.Seconds < 10 ? $"0{ts.Seconds}" : $"{ts.Seconds}")}";
        }

        public bool isSongInPlaylist(MusicContext context) => isSongInPlaylist(context.Song);
        public bool isSongInPlaylist(Song song)
        {
            if (player.getCurrentSong() != null)
                if (player.getCurrentSong().Song.VideoId.Equals(song.VideoId))
                    return true;
            if (queue.getQueue().Count(x => x.Song.VideoId.Equals(song.VideoId)) != 0)
                return true;
            return false;
        }

        public bool shouldDownload()
        {
            return false; //TODO: Fix download
            if (player.getCurrentSong() != null)
                return true;
            if (queue.getQueue().Count == 0)
                return false;
            return true;
        }
    }
}
