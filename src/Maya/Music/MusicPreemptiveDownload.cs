using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Maya.Music.MusicEnums;
using System.Net.Http;
using System.Net;

namespace Maya.Music
{
    public class MusicPreemptiveDownload
    {
        private MusicContext context;
        public MusicPreemptiveDownload(MusicContext m)
        {
            context = m;
        }

        public async Task<MusicContext> RunAsync()
        {
            switch(context.Song.Provider)
            {
                case MusicProvider.YOUTUBE:
                    {
                        return await Youtube();
                    }
            }
            return null;
        }

        private async Task<MusicContext> Youtube()
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    string url = "https://www.youtube.com/watch?v=" + context.Song.VideoId;
                    string html = await httpClient.GetStringAsync(url);
                    if (html.Contains("<meta itemprop=\"duration\" content=\""))
                    {
                        if(context.Song.Title == null)
                            context.Song.Title = WebUtility.HtmlDecode(html.Split(new string[] { "<meta itemprop=\"name\" content=\"" }, StringSplitOptions.None)[1].Split('"')[0]);
                        string dur = html.Split(new string[] { "<meta itemprop=\"duration\" content=\"" }, StringSplitOptions.None)[1].Split('"')[0];
                        int mins = int.Parse(dur.Split(new string[] { "PT" }, StringSplitOptions.None)[1].Split('M')[0]);
                        int secs = int.Parse(dur.Split('M')[1].Split('S')[0]);
                        context.Song.Duration = new TimeSpan(0, mins, secs);
                    }
                }
            }
            catch (Exception)
            {
                //... video nao encontrado = 404
                return null;
            }
            return context;
        }
    }
}
