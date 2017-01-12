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

        public string getName()
        {
            return (String)ship["name"];
        }

        public string getImageUrl()
        {
            return (string)((JObject)ship["images"])["small"];
        }

        public async Task updateData()
        {
            await Task.Delay(0);
        }

        public string getHeadStats()
        {
            return MainHandler.ShipHandler.shipNation((String)ship["nation"]) + " " + MainHandler.ShipHandler.shipType((String)ship["type"]) + " " + ship["name"] + " (Tier " + MainHandler.ShipHandler.shipTier((int)ship["tier"]) + ")";
        }

        public Task<string> getSimpleStats()
        {
            return new Task<string>(() => "RIP CVs");
        }
    }
}
