using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Maya.WoWS;
using Maya.Controllers;

namespace Maya.WoWS
{
    public class AirCarrier : IShip
    {
        private MainHandler MainHandler;
        private JObject ship;
        //private JObject ship_max;
        //private string search;
        public AirCarrier(MainHandler MainHandler, JObject o)
        {
            this.MainHandler = MainHandler;
            ship = o;
            //ship_max = null;
            //search = "";
        }

        public string GetName()
        {
            return (String)ship["name"];
        }

        public string GetImageUrl()
        {
            return (string)((JObject)ship["images"])["small"];
        }

        public async Task UpdateDataAsync()
        {
            await Task.Delay(0);
        }

        public string GetHeadStats()
        {
            return MainHandler.ShipHandler.ShipNation((String)ship["nation"]) + " " + MainHandler.ShipHandler.ShipType((String)ship["type"]) + " " + ship["name"] + " (Tier " + MainHandler.ShipHandler.ShipTier((int)ship["tier"]) + ")";
        }

        public Task<string> GetSimpleStatsAsync()
        {
            return Task.FromResult<string>("RIP CVs");
        }
    }
}
