using Discord;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Maya.GuildHandlers
{
    public class DatabaseHandler
    {
        private GuildHandler GuildHandler;
        private JObject db;
        private Timer timer;
        public DatabaseHandler(GuildHandler GuildHandler)
        {
            this.GuildHandler = GuildHandler;
            db = null;
            timer = new Timer(Timer_Elapsed, null, Timeout.Infinite, Timeout.Infinite);
        }

        public async Task Initialize()
        {
            await load();
        }

        private async void Timer_Elapsed(object state)
        {
            await save();
        }

        public async Task save()
        {
            timer.Change(Timeout.Infinite, Timeout.Infinite);
            await Task.Run(() =>
            {
                File.WriteAllText($"Configs{Path.DirectorySeparatorChar}Guilds{Path.DirectorySeparatorChar}{GuildHandler.Guild.Id}{Path.DirectorySeparatorChar}db.json", JsonConvert.SerializeObject(db));
            });
        }

        public async Task load()
        {
            await Task.Run(() =>
            {
                db = JObject.Parse(File.ReadAllText($"Configs{Path.DirectorySeparatorChar}Guilds{Path.DirectorySeparatorChar}{GuildHandler.Guild.Id}{Path.DirectorySeparatorChar}db.json"));
            });
        }

        public double getSwearJar()
        {
            if (db == null)
                return 0;
            return (double)db["SwearJar"];
        }

        public void addSwearJar(double a)
        {
            if (db == null)
                return;
            db["SwearJar"] = getSwearJar() + a;
            timer.Change(300000, Timeout.Infinite);
        }

        public string getPersonality()
        {
            if (db == null)
                return null;
            return (string)db["CurrentPersonality"];
        }

        public void setPersonality(string name)
        {
            if (db == null)
                return;
            db["CurrentPersonality"] = name;
            timer.Change(300000, Timeout.Infinite);
        }

        public List<ulong> getIgnoreList()
        {
            if (db == null)
                return null;
           return db["IgnoreList"].ToObject<List<ulong>>();
        }

        public void setIgnoreList(List<ulong> ids)
        {
            if (db == null)
                return;
            db["IgnoreList"] = JArray.FromObject(ids);
            timer.Change(300000, Timeout.Infinite);
        }

        public bool isDbReady()
        {
            return (db != null);
        }
    }
}
