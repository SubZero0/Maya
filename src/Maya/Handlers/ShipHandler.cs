using Maya.Controllers;
using Maya.Interfaces;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Maya.WoWS
{
    public class ShipHandler : IHandler
    {
        private MainHandler MainHandler;
        private Dictionary<string, IShip> ships;
        private Nullable<Boolean> ready;
        //ShipPlayer
        public Dictionary<ulong, ShipPlayer> shipPlayers;
        public ShipHandler(MainHandler MainHandler)
        {
            this.MainHandler = MainHandler;
            ships = new Dictionary<string, IShip>();
            ready = false;
            shipPlayers = new Dictionary<ulong, ShipPlayer>();
        }

        public async Task InitializeAsync()
        {
            ready = false;
            ships.Clear();
            JObject json = null;
            using (var httpClient = new HttpClient())
            {
                var jsonraw = await httpClient.GetStringAsync("https://api.worldofwarships.com/wows/encyclopedia/ships/?application_id=ca60f30d0b1f91b195a521d4aa618eee");
                json = JObject.Parse(jsonraw);
            }
            if ((String)json["status"] != "ok")
            {
                Console.WriteLine("Failed to obtain ship list.");
                ready = null;
                return;
            }
            JObject data = (JObject)json["data"];
            foreach (JObject o in data.Values())
            {
                IShip s = null;
                switch ((String)o["type"])
                {
                    case "AirCarrier": { s = new AirCarrier(MainHandler, o); break; }
                    case "Battleship": { s = new DCB(MainHandler, o); break; }
                    case "Cruiser": { s = new DCB(MainHandler, o); break; }
                    case "Destroyer": { s = new DCB(MainHandler, o); break; }
                }
                //s.updateData(); ## com await demora mt, sem await ele dá errado -.-
                ships.Add(((String)o["name"]).Replace('ū', 'u').Replace('ß', 'b').Replace('ü', 'u').Replace('ł', 'l').Replace('ö', 'o').Replace('ü', 'u').Replace('ō', 'o').ToLower(), s);
            }
            ready = true;
        }

        public Task Close()
        {
            return Task.CompletedTask;
        }

        public Nullable<Boolean> IsReady()
        {
            return ready;
        }

        public List<IShip> SearchShips(string name)
        {
            name = name.ToLower();
            List<IShip> lista = new List<IShip>();
            if (ships.ContainsKey(name))
            {
                lista.Add(ships[name]);
                return lista;
            }
            foreach (string key in ships.Keys.ToArray())
                if (key.Contains(name))
                    lista.Add(ships[key]);
            return lista;
        }

        public string[] GetShipList()
        {
            return ships.Keys.ToArray();
        }

        public string ShipNation(string nation)
        {
            switch (nation)
            {
                case "pan_asia": { return "Pan-Asian"; }
                case "commonwealth": { return "Australian"; }
                case "usa": { return "American"; }
                case "poland": { return "Polish"; }
                case "france": { return "French"; }
                case "ussr": { return "Russian"; }
                case "germany": { return "German"; }
                case "uk": { return "British"; }
                case "japan": { return "Japanese"; }
                default: { return nation; }
            }
        }

        public string ShipType(string type)
        {
            switch (type)
            {
                case "Cruiser": { return "Cruiser"; }
                case "AirCarrier": { return "Aircraft Carrier"; }
                case "Battleship": { return "Battleship"; }
                case "Destroyer": { return "Destroyer"; }
                default: { return type; }
            }
        }

        public string ShipTier(int tier)
        {
            switch (tier)
            {
                case 1: { return "I"; }
                case 2: { return "II"; }
                case 3: { return "III"; }
                case 4: { return "IV"; }
                case 5: { return "V"; }
                case 6: { return "VI"; }
                case 7: { return "VII"; }
                case 8: { return "VIII"; }
                case 9: { return "IX"; }
                case 10: { return "X"; }
                default: { return "" + tier; }
            }
        }

        public async Task<JObject> GetMaxShipAsync(string search)
        {
            using (var httpClient = new HttpClient())
            {
                string link = $"https://api.worldofwarships.com/wows/encyclopedia/shipprofile/?application_id=ca60f30d0b1f91b195a521d4aa618eee{search}";
                var jsonraw = await httpClient.GetStringAsync(link);
                JObject json = JObject.Parse(jsonraw);
                if ((String)json["status"] == "ok")
                    return (JObject)((JObject)json["data"]).Values().First();
                return null;
            }
        }

        public Dictionary<string, JObject> torpedoesCache = new Dictionary<string, JObject>();
        public async Task<JObject> GetTorpedoesAsync(string search, string torpedoes_id)
        {
            search += "&torpedoes_id=" + torpedoes_id;
            if (torpedoesCache.ContainsKey(search))
                return torpedoesCache[search];
            using (var httpClient = new HttpClient())
            {
                string link = $"https://api.worldofwarships.com/wows/encyclopedia/shipprofile/?application_id=ca60f30d0b1f91b195a521d4aa618eee{search}";
                var jsonraw = await httpClient.GetStringAsync(link);
                JObject json = JObject.Parse(jsonraw);
                if ((String)json["status"] == "ok")
                {
                    JObject ship = (JObject)((JObject)json["data"]).Values().First();
                    torpedoesCache.Add(search, ship);
                    return ship;
                }
                return null;
            }
        }
    }
}
