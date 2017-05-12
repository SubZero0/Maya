using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Maya.WoWS;
using Maya.Controllers;

namespace Maya.WoWS
{
    public class DCB : IShip
    {
        private MainHandler MainHandler;
        private JObject ship;
        private JObject ship_max;
        private string search;
        public DCB(MainHandler MainHandler, JObject o)
        {
            this.MainHandler = MainHandler;
            ship = o;
            ship_max = null;
            search = null;
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
            search = "&ship_id=" + ship["ship_id"];
            Dictionary<string, JObject> best_modules = new Dictionary<string, JObject>();
            JObject modules_tree = (JObject)ship["modules_tree"];
            foreach (JObject o in modules_tree.Values())
                if (!(bool)o["is_default"] && (string)o["type"] != "Torpedoes")
                {
                    if (!best_modules.ContainsKey((string)o["type"]))
                        best_modules[(string)o["type"]] = o;
                    else if ((int)o["price_xp"] > (int)(best_modules[(string)o["type"]])["price_xp"])
                        best_modules[(string)o["type"]] = o;
                }
            foreach (string s in best_modules.Keys)
            {
                switch (s)
                {
                    case "Hull": { search += "&hull_id="; break; }
                    case "Suo": { search += "&fire_control_id="; break; }
                    case "Artillery": { search += "&artillery_id="; break; }
                    case "Engine": { search += "&engine_id="; break; }
                    default: { continue; }
                }
                search += (best_modules[s])["module_id"];
            }
            ship_max = await MainHandler.ShipHandler.GetMaxShipAsync(search);
        }

        public string GetHeadStats()
        {
            return MainHandler.ShipHandler.ShipNation((String)ship["nation"]) + " " + MainHandler.ShipHandler.ShipType((String)ship["type"]) + " " + ship["name"] + " (Tier " + MainHandler.ShipHandler.ShipTier((int)ship["tier"]) + ")";
        }

        public async Task<string> GetSimpleStatsAsync()
        {
            if (ship_max == null)
            {
                await UpdateDataAsync();
                if (ship_max == null)
                    return "A problem occurred while getting the max stats from this ship.";
            }
            JObject artillery = (JObject)ship_max["artillery"];
            JObject armour = (JObject)ship_max["armour"];
            JObject mobility = (JObject)ship_max["mobility"];
            JObject concealment = (JObject)ship_max["concealment"];
            JObject artillery_shells = (JObject)artillery["shells"];
            JObject artillery_slots_0 = (JObject)((JObject)artillery["slots"])["0"];
            string simple = "";
            simple += "HP: " + ((JObject)ship_max["hull"])["health"] + "\n";
            if ((String)armour["flood_prob"] != "0")
                simple += "Torpedo protection (flooding): " + armour["flood_prob"] + "%\n";
            if ((String)armour["flood_damage"] != "0")
                simple += "Torpedo protection (damage): " + armour["flood_damage"] + "%\n";
            simple += "**Artillery**:\n";
            simple += "- Main battery: " + artillery_slots_0["name"] + " (" + artillery_slots_0["guns"] + "x" + artillery_slots_0["barrels"] + ")\n";
            simple += "- Reload: " + artillery["shot_delay"] + "s\n";
            simple += "- Range: " + artillery["distance"] + "km\n";
            simple += "- Dispersion: " + artillery["max_dispersion"] + "m\n";
            simple += "- Damage: [AP: " + (artillery_shells["AP"] != null ? ((JObject)artillery_shells["AP"])["damage"] : "-") + ", HE: " + (artillery_shells["HE"] != null ? ((JObject)artillery_shells["HE"])["damage"] : "-") + "]\n";
            if (artillery_shells["HE"] != null)
                simple += "- Fire chance: " + ((JObject)artillery_shells["HE"])["burn_probability"] + "%\n";
            JArray torpedoes = (JArray)((JObject)ship["modules"])["torpedoes"];
            if (torpedoes.Count() != 0)
            {
                simple += "**Torpedoes**:\n";
                JObject default_torpedoes = (JObject)ship["default_profile"];
                string default_torpedo_id = (string)((JObject)default_torpedoes["torpedoes"])["torpedoes_id"];
                foreach (string torpedoes_id in torpedoes)
                {
                    JObject gtorps = null;
                    if (torpedoes_id == default_torpedo_id)
                        gtorps = default_torpedoes;
                    else
                        gtorps = await MainHandler.ShipHandler.GetTorpedoesAsync(search, torpedoes_id);
                    if (gtorps == null)
                        continue;
                    JObject torps = (JObject)gtorps["torpedoes"];
                    JObject torpedoes_slots = (JObject)torps["slots"];
                    JObject slots_0 = (JObject)torpedoes_slots["0"];
                    simple += "- " + torps["torpedo_name"] + " (" + slots_0["guns"] + "x" + slots_0["barrels"] + "): [MaxDamage: " + torps["max_damage"] + "; Reload: " + torps["reload_time"] + "s; Speed: " + torps["torpedo_speed"] + " kts; Range: " + torps["distance"] + "km; Visibility: " + torps["visibility_dist"] + "km]\n";
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
