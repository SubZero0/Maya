using Maya.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Maya.GuildHandlers
{
    public class DatabaseHandler : IGuildHandler
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

        public async Task InitializeAsync()
        {
            await LoadAsync();
        }

        public async Task Close()
        {
            await SaveAsync();
            timer.Dispose();
        }

        private async void Timer_Elapsed(object state)
        {
            await SaveAsync();
        }

        public async Task SaveAsync()
        {
            timer.Change(Timeout.Infinite, Timeout.Infinite);
            await Task.Run(() =>
            {
                File.WriteAllText($"Configs{Path.DirectorySeparatorChar}Guilds{Path.DirectorySeparatorChar}{GuildHandler.Guild.Id}{Path.DirectorySeparatorChar}db.json", JsonConvert.SerializeObject(db));
            });
        }

        public async Task LoadAsync()
        {
            await Task.Run(() =>
            {
                db = JObject.Parse(File.ReadAllText($"Configs{Path.DirectorySeparatorChar}Guilds{Path.DirectorySeparatorChar}{GuildHandler.Guild.Id}{Path.DirectorySeparatorChar}db.json"));
            });
        }

        public double GetSwearJar()
        {
            if (db == null)
                return 0;
            return (double)db["SwearJar"];
        }

        public void AddSwearJar(double a)
        {
            if (db == null)
                return;
            db["SwearJar"] = GetSwearJar() + a;
            timer.Change(300000, Timeout.Infinite);
        }

        public string GetPersonality()
        {
            if (db == null)
                return null;
            return (string)db["CurrentPersonality"];
        }

        public void SetPersonality(string name)
        {
            if (db == null)
                return;
            db["CurrentPersonality"] = name;
            timer.Change(300000, Timeout.Infinite);
        }

        public List<ulong> GetIgnoreList()
        {
            if (db == null)
                return null;
           return db["IgnoreList"].ToObject<List<ulong>>();
        }

        public void SetIgnoreList(List<ulong> ids)
        {
            if (db == null)
                return;
            db["IgnoreList"] = JArray.FromObject(ids);
            timer.Change(300000, Timeout.Infinite);
        }

        public bool IsReady()
        {
            return (db != null);
        }
    }
}
