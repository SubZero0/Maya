using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Maya.Handlers
{
    public class ConfigHandler
    {
        private JObject config;
        public ConfigHandler()
        {
            config = null;
        }

        public async Task Initialize()
        {
            await load();
        }

        public async Task load()
        {
            await Task.Run(() =>
            {
                config = JObject.Parse(File.ReadAllText($"Configs{Path.DirectorySeparatorChar}config.json"));
            });
        }

        public string getBotToken()
        {
            if (config == null)
                return "";
            return (string)config["BotToken"];
        }

        public string getDefaultCommandPrefix()
        {
            if (config == null)
                return "";
            return (string)config["DefaultCommandPrefix"];
        }

        public string[,] getAnswers()
        {
            if (config == null)
                return new string[,] { };
            return ((JArray)config["Answers"]).ToObject<string[,]>();
        }

        public int getSwearTimer()
        {
            if (config == null)
                return 1;
            return (int)config["SwearTimer"];
        }

        public string getSwearString()
        {
            if (config == null)
                return "";
            return (string)config["SwearString"];
        }
    }
}
