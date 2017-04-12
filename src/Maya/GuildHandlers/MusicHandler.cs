using Discord;
using Discord.WebSocket;
using Maya.Interfaces;
using Maya.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Maya.GuildHandlers
{
    public class MusicHandler : IGuildHandler
    {
        private GuildHandler GuildHandler;
        private IVoiceChannel voiceChannel;
        private MusicQueue queue;
        private MusicPlayer player;
        private readonly int maxQueue = 10;
        public MusicHandler(GuildHandler GuildHandler)
        {
            this.GuildHandler = GuildHandler;
            voiceChannel = null;
            queue = new MusicQueue(this);
            player = new MusicPlayer(GuildHandler, this);
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task Close()
        {
            try
            {
                Stop();
                if (player?.audioClient != null)
                    await player.audioClient.StopAsync();
            }
            catch { }
        }

        public bool IsReady()
        {
            if (queue == null)
                return false;
            if (player == null)
                return false;
            return true;
        }

        public MusicResult CanUserUseMusicCommands(MusicContext context)
        {
            if (!(context.AskedBy is IGuildUser))
                return new MusicResult("null");
            var wrapper = GuildHandler.ConfigHandler.GetMusic();
            if (!wrapper.IsEnabled)
                return new MusicResult("null");
            if(!Utils.IsChannelListed(context.Channel, wrapper.TextChannels))
                return new MusicResult("null");
            return new MusicResult();
        }

        public MusicResult CanUserAddToQueue(MusicContext context, bool checkCmds = true)
        {
            if (checkCmds)
            {
                MusicResult mr = CanUserUseMusicCommands(context);
                if (!mr.IsSuccessful)
                    return mr;
            }
            if ((context.AskedBy as IGuildUser).VoiceChannel != voiceChannel)
                return new MusicResult($"You need to be in the same music **voice** channel as the bot ({voiceChannel.Name}).");
            return new MusicResult();
        }

        public MusicQueue GetMusicQueue()
        {
            return queue;
        }

        public MusicContext GetCurrentSong()
        {
            return player.GetCurrentSong();
        }

        public IVoiceChannel GetVoiceChannel()
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

        public async Task JoinVoiceChannelAsync(IVoiceChannel channel)
        {
            if (channel == null)
                return;
            if (player.audioClient != null)
                await player.audioClient.StopAsync();
            voiceChannel = channel;
            player.audioClient = await channel.ConnectAsync();
        }

        public async Task ResetAsync()
        {
            //TODO: Clear voice buffer?
            Stop();
            await Task.Delay(1000);
            await JoinVoiceChannelAsync(voiceChannel);
            await GuildHandler.MainHandler.Client.SetGameAsync(null);
            player = new MusicPlayer(GuildHandler, this);
        }

        public void ChangeVolume(int n) //[0,100]
        {
            player.ChangeVolume(n / 100.0f);
        }

        public int GetVolume() //[0,100]
        {
            return (int)(player.GetVolume() * 100);
        }

        public async Task SearchAsync(MusicContext music, string search)
        {
            music = await new MusicSearch(music, search).RunAsync();
            if (music == null)
            {
                await music.Channel.SendMessageAsync("No result found.");
                return;
            }
            if (IsSongInPlaylist(music))
            {
                await music.Channel.SendMessageAsync($"Couldn't queue your request of **{music.Song.Title ?? music.Song.VideoId}** because it's already in the playlist.");
                return;
            }
            music = await new MusicPreemptiveDownload(music).RunAsync();
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
            await QueueAsync(music);
        }

        public async Task QueueAsync(MusicContext music)
        {
            if (queue.GetQueue().Count >= maxQueue)
            {
                await music.Channel.SendMessageAsync($"Couldn't queue your request of **{music.Song.Title}** because the limit was reached. (Max: {maxQueue})");
                return;
            }
            if (IsSongInPlaylist(music))
            {
                await music.Channel.SendMessageAsync($"Couldn't queue your request of **{music.Song.Title ?? music.Song.VideoId}** because it's already in the playlist.");
                return;
            }
            await queue.EnqueueAsync(music);
            await music.Channel.SendMessageAsync($"Enqueued **{music.Song.Title}** to the playlist.\nPosition: {queue.GetQueue().Count} - estimated time until playing: {GetEstimatedTimeUntil(music)}");
            await PlayAsync();
        }

        public async Task PlayAsync()
        {
            await player.RunAsync();
        }

        public string GetEstimatedTimeUntil(MusicContext until)
        {
            int secs = 0;
            if (player.GetCurrentSong() != null)
                secs += player.GetCurrentSong().Song.GetTimeUntilEnd();
            foreach (MusicContext m in queue.GetQueue())
            {
                if (m == until)
                    break;
                secs += (int)m.Song.Duration?.TotalSeconds;
            }
            TimeSpan ts = new TimeSpan(0, 0, secs);
            return $"{ts.Hours}:{(ts.Minutes < 10 ? $"0{ts.Minutes}" : $"{ts.Minutes}")}:{(ts.Seconds < 10 ? $"0{ts.Seconds}" : $"{ts.Seconds}")}";
        }

        public bool IsSongInPlaylist(MusicContext context) => IsSongInPlaylist(context.Song);
        public bool IsSongInPlaylist(Song song)
        {
            if (player.GetCurrentSong() != null)
                if (player.GetCurrentSong().Song.VideoId.Equals(song.VideoId))
                    return true;
            if (queue.GetQueue().Count(x => x.Song.VideoId.Equals(song.VideoId)) != 0)
                return true;
            return false;
        }

        public bool ShouldDownload()
        {
            return false; //TODO: Fix download
            if (player.GetCurrentSong() != null)
                return true;
            if (queue.GetQueue().Count == 0)
                return false;
            return true;
        }
    }
}
