using Discord.Audio;
using Maya.GuildHandlers;
using Maya.Music.Youtube;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Maya.Music.MusicEnums;

namespace Maya.Music
{
    public class MusicPlayer
    {
        public IAudioClient audioClient;
        private GuildHandler GuildHandler;
        private MusicHandler MusicHandler;
        private MusicContext currentSong;
        private Process mProcess;
        private float volume;
        public MusicPlayer(GuildHandler GuildHandler, MusicHandler MusicHandler)
        {
            this.GuildHandler = GuildHandler;
            this.MusicHandler = MusicHandler;
            audioClient = null;
            currentSong = null;
            mProcess = null;
            volume = 1.0f;
        }

        public async Task RunAsync()
        {
            if (currentSong != null)
                return;
            await GuildHandler.MainHandler.Client.SetGameAsync(null);
            MusicContext music = MusicHandler.GetMusicQueue().GetNextSong();
            if (music == null)
                return;
            //await MusicHandler.JoinVoiceChannel(await Utils.findVoiceChannel((music.AskedBy as Discord.IGuildUser).Guild as Discord.WebSocket.SocketGuild, GuildHandler.MainHandler.GuildConfigHandler(GuildHandler.Guild).getMusic().VoiceChannel));
            currentSong = music;
            while (currentSong.Song.Status == MusicStatus.DOWNLOADING) ;
            if(currentSong.Song.Status == MusicStatus.BROKEN)
            {
                currentSong = null;
                await music.Channel.SendMessageAsync($"Something went wrong when downloading **{music.Song.Title}**. Skipping...");
                await RunAsync();
                return;
            }
            await PlayAudioAsync(music);
        }

        public MusicContext GetCurrentSong()
        {
            return currentSong;
        }

        public void ChangeVolume(float f) //[0,1]
        {
            if (f < 0)
                f = 0;
            else if (f > 1)
                f = 1;
            volume = f;
        }

        public float GetVolume() //[0,1]
        {
            return volume;
        }

        public void Skip()
        {
            if (mProcess != null)
                mProcess.Kill();
        }

        public async Task PlayAudioAsync(MusicContext m)
        {
            currentSong = m;
            string path = null;
            if (m.Song.Status == MusicStatus.NEW)
            {
                try
                {
                    string url = "https://www.youtube.com/watch?v=" + m.Song.VideoId;
                    IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(url);
                    VideoInfo video = videoInfos.First(info => info.VideoType == VideoType.Mp4 && info.Resolution == 360);
                    if (video.RequiresDecryption)
                        DownloadUrlResolver.DecryptDownloadUrl(video);
                    path = video.DownloadUrl;
                }
                catch (Exception)
                {
                    await m.Channel.SendMessageAsync($"Something went wrong when reading **{m.Song.Title}**. Skipping...");
                    goto End;
                }
            }
            else if (m.Song.Status == MusicStatus.DOWNLOADED)
            {
                path = $"Temp{Path.DirectorySeparatorChar}{m.Song.VideoId}.mp4";
            }
            else
                goto End;
            using (var stream = audioClient.CreatePCMStream(AudioApplication.Music))
            {
                mProcess = Process.Start(new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = "-loglevel quiet " +
                    $"-i \"{path}\" " +
                    "-f s16le -ar 48000 -ac 2 pipe:1",
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = false
                });
                await Task.Delay(2000);
                if (m.AskedBy != null)
                    await m.Channel.SendMessageAsync($"Hey {m.AskedBy.Mention}, your request of **{m.Song.Title}** will play now!");
                await GuildHandler.MainHandler.Client.SetGameAsync(m.Song.Title);
                m.Song.StartTime = DateTime.Now;
                while (true)
                {
                    if (mProcess.HasExited)
                        break;
                    int blockSize = 2880;
                    byte[] buffer = new byte[blockSize];
                    int byteCount;
                    byteCount = await mProcess.StandardOutput.BaseStream.ReadAsync(buffer, 0, blockSize);
                    if (byteCount == 0)
                        break;
                    await stream.WriteAsync(ScaleVolumeSafeAllocateBuffers(buffer, volume), 0, byteCount);
                }
                await stream.FlushAsync();
            }
            End:
            await Task.Delay(500);
            if (File.Exists(path))
                File.Delete(path);
            mProcess = null;
            currentSong = null;
            await RunAsync();
        }

        public byte[] ScaleVolumeSafeAllocateBuffers(byte[] audioSamples, float volume)
        {
            if (audioSamples == null) return null;
            if (audioSamples.Length % 2 != 0) return null;
            if (volume < 0f || volume > 1f) return null;
            var output = new byte[audioSamples.Length];
            if (Math.Abs(volume - 1f) < 0.0001f)
            {
                Buffer.BlockCopy(audioSamples, 0, output, 0, audioSamples.Length);
                return output;
            }
            int volumeFixed = (int)Math.Round(volume * 65536d);
            for (var i = 0; i < output.Length; i += 2)
            {
                int sample = (short)((audioSamples[i + 1] << 8) | audioSamples[i]);
                int processed = (sample * volumeFixed) >> 16;
                output[i] = (byte)processed;
                output[i + 1] = (byte)(processed >> 8);
            }
            return output;
        }
    }
}
