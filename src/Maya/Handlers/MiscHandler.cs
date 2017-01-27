using Newtonsoft.Json;
using System;
using System.IO;

namespace Maya.Handlers
{
    public class MiscHandler
    {
        public string AppName = "";
        public string GoogleAPIkey = "";
        public string BingAPIKey = "";
        public string MashAPIkey = "";
        public string GifyAPIKey = "";
        public ulong ExeGuild = 0;
        public ulong ExeChannel = 0;

        public static readonly string appdir = AppContext.BaseDirectory;
        public void Save(string dir = "Configs/Creds.json")
        {
            string file = Path.Combine(appdir, dir);
            File.WriteAllText(file, ToJson());
        }

        public static MiscHandler Load(string dir = "Configs/Creds.json")
        {
            string file = Path.Combine(appdir, dir);
            return JsonConvert.DeserializeObject<MiscHandler>(File.ReadAllText(file));
        }

        public string ToJson()
            => JsonConvert.SerializeObject(this, Formatting.Indented);
    }
}
