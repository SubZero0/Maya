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
        private JObject shipMax;
        private List<JObject> cv;
        public AirCarrier(MainHandler MainHandler, JObject o)
        {
            this.MainHandler = MainHandler;
            ship = o;
            shipMax = null;
            cv = null;
        }

        public string GetName()
        {
            return (String)ship["name"];
        }

        public ulong GetId()
        {
            return (ulong)ship["ship_id"];
        }

        public string GetImageUrl()
        {
            return (string)((JObject)ship["images"])["small"];
        }

        public async Task UpdateDataAsync()
        {
            string search = "&ship_id=" + ship["ship_id"];
            Dictionary<string, JObject> best_modules = new Dictionary<string, JObject>();
            List<string> cv_modules = new List<string>();
            JObject modules_tree = (JObject)ship["modules_tree"];
            foreach (JObject o in modules_tree.Values())
                if (!(bool)o["is_default"] && (string)o["type"] != "Torpedoes")
                {
                    string type = (string)o["type"];
                    if (type == "FlightControl" || type == "Fighter" || type == "DiveBomber" || type == "TorpedoBomber")
                        cv_modules.Add((string)o["module_id"]);
                    else
                    {
                        if (!best_modules.ContainsKey(type))
                            best_modules[type] = o;
                        else if ((int)o["price_xp"] > (int)(best_modules[type])["price_xp"])
                            best_modules[type] = o;
                    }
                }
            foreach (string s in best_modules.Keys)
            {
                string str = (string)(best_modules[s])["module_id"];
                switch (s)
                {
                    case "Hull": { search += $"&hull_id={str}"; break; }
                    case "Engine": { search += $"&engine_id={str}"; break; }
                }
            }
            shipMax = await MainHandler.ShipHandler.GetMaxShipAsync(search);
            cv = await MainHandler.ShipHandler.GetCVAsync(cv_modules);
        }

        public string GetHeadStats()
        {
            return MainHandler.ShipHandler.ShipNation((String)ship["nation"]) + " " + MainHandler.ShipHandler.ShipType((String)ship["type"]) + " " + ship["name"] + " (Tier " + MainHandler.ShipHandler.ShipTier((int)ship["tier"]) + ")";
        }

        public async Task<string> GetSimpleStatsAsync()
        {
            if (shipMax == null || cv == null)
            {
                await UpdateDataAsync();
                if (shipMax == null)
                    return "A problem occurred while getting the max stats from this ship.";
                if (cv == null)
                    return "A problem occurred while getting the cv stats.";
            }
            JObject armour = (JObject)shipMax["armour"];
            JObject mobility = (JObject)shipMax["mobility"];
            JObject concealment = (JObject)shipMax["concealment"];
            string simple = "";
            simple += "HP: " + ((JObject)shipMax["hull"])["health"] + "\n";
            simple += "Planes: " + ((JObject)shipMax["hull"])["planes_amount"] + "\n";
            if ((String)armour["flood_prob"] != "0")
                simple += "Torpedo protection (flooding): " + armour["flood_prob"] + "%\n";
            if ((String)armour["flood_damage"] != "0")
                simple += "Torpedo protection (damage): " + armour["flood_damage"] + "%\n";

            simple += "**Flight control** (F/TB/DB):\n";
            JObject flight_control = (JObject)shipMax["flight_control"];
            simple += $"- {flight_control["fighter_squadrons"]}/{flight_control["torpedo_squadrons"]}/{flight_control["bomber_squadrons"]}\n";
            foreach (JObject module in cv.Where(x => (string)x["type"] == "FlightControl"))
            {
                flight_control = (JObject)module["profile"]["flight_control"];
                simple += $"- {flight_control["fighter_squadrons"]}/{flight_control["torpedo_squadrons"]}/{flight_control["bomber_squadrons"]}\n";
            }
            if (shipMax["fighters"] != null && shipMax["fighters"].HasValues)
            {
                simple += "**Fighter**:\n";
                JObject fighters = (JObject)shipMax["fighters"];
                simple += $"- {fighters["name"]}: [Dmg: {fighters["avg_damage"]}, HP: {fighters["max_health"]}, Speed: {fighters["cruise_speed"]}, Ammo: {fighters["max_ammo"]}]\n";
                foreach (JObject module in cv.Where(x => (string)x["type"] == "Fighter"))
                {
                    fighters = (JObject)module["profile"]["fighter"];
                    simple += $"- {module["name"]}: [Dmg: {fighters["avg_damage"]}, HP: {fighters["max_health"]}, Speed: {fighters["cruise_speed"]}, Ammo: {fighters["max_ammo"]}]\n";
                }
            }
            if (shipMax["torpedo_bomber"] != null && shipMax["torpedo_bomber"].HasValues)
            {
                simple += "**Torpedo bomber**:\n";
                JObject tbs = (JObject)shipMax["torpedo_bomber"];
                simple += $"- {tbs["name"]}: [Dmg: {tbs["max_damage"]}, HP: {tbs["max_health"]}, Speed: {tbs["cruise_speed"]}, TorpSpeed: {tbs["torpedo_max_speed"]} kts]\n";
                foreach (JObject module in cv.Where(x => (string)x["type"] == "TorpedoBomber"))
                {
                    tbs = (JObject)module["profile"]["torpedo_bomber"];
                    simple += $"- {module["name"]}: [Dmg: {tbs["max_damage"]}, HP: {tbs["max_health"]}, Speed: {tbs["cruise_speed"]}, TorpSpeed: {tbs["torpedo_max_speed"]} kts]\n";
                }
            }
            if (shipMax["dive_bomber"] != null && shipMax["dive_bomber"].HasValues)
            {
                simple += "**Dive bomber**:\n";
                JObject dbs = (JObject)shipMax["dive_bomber"];
                simple += $"- {dbs["name"]}: [Dmg: {dbs["max_damage"]}, HP: {dbs["max_health"]}, Speed: {dbs["cruise_speed"]}]\n";
                foreach (JObject module in cv.Where(x => (string)x["type"] == "DiveBomber"))
                {
                    dbs = (JObject)module["profile"]["dive_bomber"];
                    simple += $"- {module["name"]}: [Dmg: {dbs["max_damage"]}, HP: {dbs["max_health"]}, Speed: {dbs["cruise_speed"]}]\n";
                }
            }
            simple += "**Mobility**:\n";
            simple += "- Speed: " + mobility["max_speed"] + " kts\n";
            simple += "- Rudder shift time: " + mobility["rudder_time"] + "s\n";
            simple += "- Turning radius: " + mobility["turning_radius"] + "m\n";
            simple += "**Concealment**:\n";
            simple += "- By ship: " + concealment["detect_distance_by_ship"] + "km\n";
            simple += "- By plane: " + concealment["detect_distance_by_plane"] + "km";
            return simple;
        }
    }
}
