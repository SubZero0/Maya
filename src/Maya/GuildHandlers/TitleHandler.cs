using Discord;
using Maya.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Maya.GuildHandlers
{
    public class TitleHandler : IGuildHandler
    {
        private GuildHandler GuildHandler;
        private Dictionary<ulong, List<string>> titles = null;
        private Timer timer;
        public TitleHandler(GuildHandler GuildHandler)
        {
            this.GuildHandler = GuildHandler;
        }

        public async Task InitializeAsync()
        {
            await LoadTitlesAsync();
            timer = new Timer(TimerElapsed, null, Timeout.Infinite, Timeout.Infinite);
        }

        public async Task Close()
        {
            await SaveTitlesAsync();
            timer.Dispose();
        }

        private async void TimerElapsed(object state)
        {
            await SaveTitlesAsync();
        }

        public async Task SaveTitlesAsync()
        {
            await Task.Run(() =>
            {
                Dictionary<ulong, List<string>> temp = new Dictionary<ulong, List<string>>(titles);
                File.WriteAllText($"Configs{Path.DirectorySeparatorChar}Guilds{Path.DirectorySeparatorChar}{GuildHandler.Guild.Id}{Path.DirectorySeparatorChar}titles.json", JsonConvert.SerializeObject(temp));
            });
        }

        public async Task LoadTitlesAsync()
        {
            await Task.Run(() =>
            {
                Dictionary<ulong, List<string>> temp;
                temp = JsonConvert.DeserializeObject<Dictionary<ulong, List<string>>>(File.ReadAllText($"Configs{Path.DirectorySeparatorChar}Guilds{Path.DirectorySeparatorChar}{GuildHandler.Guild.Id}{Path.DirectorySeparatorChar}titles.json"));
                titles = temp;
            });
        }

        public void AddTitle(IUser u, string title)
        {
            if (!titles.ContainsKey(u.Id))
            {
                List<string> l = new List<string>();
                l.Add(title);
                titles[u.Id] = l;
            }
            else
                titles[u.Id].Add(title);
            timer.Change(300000, Timeout.Infinite);
        }

        public void RemoveTitle(IUser u, string title)
        {
            if(ContainsTitle(u, title))
            {
                titles[u.Id].Remove(title);
                if (titles[u.Id].Count == 0)
                    titles.Remove(u.Id);
            }
            timer.Change(300000, Timeout.Infinite);
        }

        public bool ContainsTitle(IUser u, string title)
        {
            if (!titles.ContainsKey(u.Id))
                return false;
            return titles[u.Id].Contains(title);
        }

        public List<string> GetTitles(IUser u)
        {
            if (!titles.ContainsKey(u.Id))
                return new List<string>();
            return titles[u.Id];
        }
    }
}
