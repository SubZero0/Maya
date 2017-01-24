using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Maya.Music.MusicEnums;

namespace Maya.Music
{
    public class MusicSearch
    {
        private MusicContext context;
        private string search;
        public MusicSearch(MusicContext m, string search)
        {
            context = m;
            this.search = search;
        }

        public async Task<MusicContext> RunAsync()
        {
            if (await YoutubeSearch())
                return context;
            return null;
        }

        private async Task<bool> YoutubeSearch()
        {
            YouTubeService youtube = new YouTubeService(new BaseClientService.Initializer()
            {
                ApplicationName = MiscHandler.Load().AppName,
                ApiKey = MiscHandler.Load().GoogleAPIkey
            });
            SearchResource.ListRequest listRequest = youtube.Search.List("snippet");
            listRequest.Q = search;
            listRequest.MaxResults = 1;
            listRequest.Type = "video";
            SearchListResponse resp = await listRequest.ExecuteAsync();
            if (resp.Items.Count() == 0)
                return false;
            SearchResult result = resp.Items.ElementAt(0);
            context.Song.VideoId = result.Id.VideoId;
            context.Song.Title = result.Snippet.Title;
            context.Song.Provider = MusicProvider.YOUTUBE;
            return true;
        }

        private string parseYoutube(string text)
        {
            if (text.Contains("youtube.com") && text.Contains("v="))
            {
                text = text.Split(new string[] { "v=" }, StringSplitOptions.None)[1];
                if (text.Contains('&'))
                    text = text.Split('&')[0];
            }
            else if (text.Contains("youtu.be/"))
            {
                text = text.Split(new string[] { "youtu.be/" }, StringSplitOptions.None)[1];
                if (text.Contains('?'))
                    text = text.Split('?')[0];
            }
            return text;
        }
    }
}
