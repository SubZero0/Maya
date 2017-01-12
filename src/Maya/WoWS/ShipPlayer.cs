using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Maya.WoWS
{
    public class ShipPlayer
    {
        public ulong userId;
        public DateTime date;
        public ShipPlayer(ulong id)
        {
            userId = id;
            date = DateTime.Now;
        }
    }
}
