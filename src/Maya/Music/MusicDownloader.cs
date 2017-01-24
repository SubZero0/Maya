using Maya.Music.Youtube;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using static Maya.Music.MusicEnums;

namespace Maya.Music
{
    public class MusicDownloader
    {
        private MusicContext context;
        public MusicDownloader(MusicContext music)
        {
            context = music;
            context.Song.Status = MusicStatus.DOWNLOADING;
        }

        public async Task RunAsync()
        {
            switch (context.Song.Provider)
            {
                case MusicProvider.YOUTUBE:
                    {
                        await Youtube();
                        break;
                    }
            }
        }

        private async Task Youtube()
        {
            try
            {
                string path = "Temp" + Path.DirectorySeparatorChar + context.Song.VideoId + ".mp4";
                string url = "https://www.youtube.com/watch?v=" + context.Song.VideoId;
                IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(url);
                VideoInfo video = videoInfos.First(info => info.VideoType == VideoType.Mp4 && info.Resolution == 360);
                if (video.RequiresDecryption)
                    DownloadUrlResolver.DecryptDownloadUrl(video);
                using (var http = new HttpClient())
                {
                    using (var source = await http.GetStreamAsync(video.DownloadUrl))
                    {
                        using (FileStream target = File.Open(path, FileMode.Create, FileAccess.Write))
                        {
                            var buffer = new byte[1024];
                            bool cancel = false;
                            int bytes;
                            int copiedBytes = 0;
                            while (!cancel && (bytes = source.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                target.Write(buffer, 0, bytes);
                                copiedBytes += bytes;
                            }
                        }
                    }
                }
                await Task.Delay(250);
                context.Song.Status = MusicStatus.DOWNLOADED;
            }
            catch(Exception)
            {
                context.Song.Status = MusicStatus.BROKEN;
            }
        }
    }
}
