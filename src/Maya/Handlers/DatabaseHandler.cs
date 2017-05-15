using Maya.Controllers;
using Maya.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Maya.Handlers
{
    public class DatabaseHandler : IHandler
    {
        private MainHandler MainHandler;
        private JObject cookieDb;
        private Timer timer;
        public DatabaseHandler(MainHandler MainHandler)
        {
            this.MainHandler = MainHandler;
            cookieDb = null;
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
                File.WriteAllText($"Configs{Path.DirectorySeparatorChar}cookies.json", JsonConvert.SerializeObject(cookieDb));
            });
        }

        public async Task LoadAsync()
        {
            await Task.Run(() =>
            {
                cookieDb = JObject.Parse(File.ReadAllText($"Configs{Path.DirectorySeparatorChar}cookies.json"));
            });
        }

        public long GetCookies(ulong id)
        {
            if (cookieDb == null)
                return 0;
            if (cookieDb[$"{id}"] == null)
                return 0;
            return (long)cookieDb[$"{id}"]["amount"];
        }

        public void AddCookies(ulong id, long value)
        {
            if (cookieDb == null)
                return;
            if (cookieDb[$"{id}"] == null)
            {
                cookieDb[$"{id}"] = new JObject
                {
                    ["amount"] = value,
                    ["lastBake"] = DateTime.UtcNow
                };
            }
            else
                cookieDb[$"{id}"]["amount"] = GetCookies(id) + value;
            timer.Change(300000, Timeout.Infinite);
        }

        public DateTime? GetLastBake(ulong id)
        {
            if (cookieDb == null)
                return null;
            if (cookieDb[$"{id}"] == null)
                return null;
            return (DateTime)cookieDb[$"{id}"]["lastBake"];
        }

        public void UpdateLastBake(ulong id)
        {
            if (cookieDb == null)
                return;
            if (cookieDb[$"{id}"] == null)
            {
                cookieDb[$"{id}"] = new JObject
                {
                    ["amount"] = 0,
                    ["lastBake"] = DateTime.UtcNow.AddHours(2)
                };
            }
            else
                cookieDb[$"{id}"]["lastBake"] = DateTime.UtcNow.AddHours(2);
            timer.Change(300000, Timeout.Infinite);
        }

        public bool IsReady()
        {
            return (cookieDb != null);
        }
    }
}
