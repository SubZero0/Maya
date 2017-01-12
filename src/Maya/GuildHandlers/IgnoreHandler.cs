using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Maya.GuildHandlers
{
    public class IgnoreHandler
    {
        private GuildHandler GuildHandler;
        private List<ulong> ignore_list;
        public IgnoreHandler(GuildHandler GuildHandler)
        {
            this.GuildHandler = GuildHandler;
            ignore_list = new List<ulong>();
        }

        public void Initialize()
        {
            ignore_list.AddRange(GuildHandler.DatabaseHandler.getIgnoreList());
        }

        public void save()
        {
            GuildHandler.DatabaseHandler.setIgnoreList(ignore_list);
        }

        public void add(ulong id)
        {
            ignore_list.Add(id);
            save();
        }

        public void remove(ulong id)
        {
            if (contains(id))
            {
                ignore_list.Remove(id);
                save();
            }
        }

        public bool contains(ulong id)
        {
            return ignore_list.Contains(id);
        }
    }
}
