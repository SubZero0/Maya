using Discord;
using Maya.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Maya.GuildHandlers
{
    public class IgnoreHandler : IGuildHandler
    {
        private GuildHandler GuildHandler;
        private List<ulong> ignoreList;
        public IgnoreHandler(GuildHandler GuildHandler)
        {
            this.GuildHandler = GuildHandler;
            ignoreList = new List<ulong>();
        }

        public Task InitializeAsync()
        {
            ignoreList.AddRange(GuildHandler.DatabaseHandler.GetIgnoreList());
            return Task.CompletedTask;
        }

        public Task Close()
        {
            return Task.CompletedTask;
        }

        public void Save()
        {
            GuildHandler.DatabaseHandler.SetIgnoreList(ignoreList);
        }

        public void Add(ulong id)
        {
            ignoreList.Add(id);
            Save();
        }

        public void Remove(ulong id)
        {
            if (Contains(id))
            {
                ignoreList.Remove(id);
                Save();
            }
        }

        public bool Contains(ulong id)
        {
            return ignoreList.Contains(id);
        }
    }
}
