using Discord;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Maya.GuildHandlers
{
    public class TitleHandler
    {
        private GuildHandler GuildHandler;
        private Dictionary<ulong, List<string>> titles = null;
        private Timer timer;
        public TitleHandler(GuildHandler GuildHandler)
        {
            this.GuildHandler = GuildHandler;
        }

        public async Task Initialize()
        {
            await loadTitles();
            timer = new Timer(Timer_Elapsed, null, Timeout.Infinite, Timeout.Infinite);
        }

        private async void Timer_Elapsed(object state)
        {
            await saveTitles();
        }

        public async Task saveTitles()
        {
            await Task.Run(() =>
            {
                Dictionary<ulong, List<string>> temp = new Dictionary<ulong, List<string>>(titles);
                File.WriteAllText($"Configs{Path.DirectorySeparatorChar}Guilds{Path.DirectorySeparatorChar}{GuildHandler.Guild.Id}{Path.DirectorySeparatorChar}titles.json", JsonConvert.SerializeObject(temp));
            });
        }

        public async Task loadTitles()
        {
            await Task.Run(() =>
            {
                Dictionary<ulong, List<string>> temp;
                temp = JsonConvert.DeserializeObject<Dictionary<ulong, List<string>>>(File.ReadAllText($"Configs{Path.DirectorySeparatorChar}Guilds{Path.DirectorySeparatorChar}{GuildHandler.Guild.Id}{Path.DirectorySeparatorChar}titles.json"));
                titles = temp;
            });
        }

        public void addTitle(IUser u, string title)
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

        public void removeTitle(IUser u, string title)
        {
            if(containsTitle(u, title))
            {
                titles[u.Id].Remove(title);
                if (titles[u.Id].Count == 0)
                    titles.Remove(u.Id);
            }
            timer.Change(300000, Timeout.Infinite);
        }

        public bool containsTitle(IUser u, string title)
        {
            if (!titles.ContainsKey(u.Id))
                return false;
            return titles[u.Id].Contains(title);
        }

        public List<string> getTitles(IUser u)
        {
            if (!titles.ContainsKey(u.Id))
                return new List<string>();
            return titles[u.Id];
        }
    }
}
