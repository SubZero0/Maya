using Maya.Interfaces;
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
    public class ConfigHandler : IHandler
    {
        private JObject config;
        public ConfigHandler()
        {
            config = null;
        }

        public async Task InitializeAsync()
        {
            await LoadAsync();
        }

        public Task Close()
        {
            return Task.CompletedTask;
        }

        public async Task LoadAsync()
        {
            await Task.Run(() =>
            {
                config = JObject.Parse(File.ReadAllText($"Configs{Path.DirectorySeparatorChar}config.json"));
            });
        }

        public string GetBotToken()
        {
            if (config == null)
                return "";
            return (string)config["BotToken"];
        }

        public string GetDefaultCommandPrefix()
        {
            if (config == null)
                return "";
            return (string)config["DefaultCommandPrefix"];
        }

        public string[,] GetAnswers()
        {
            if (config == null)
                return new string[,] { };
            return ((JArray)config["Answers"]).ToObject<string[,]>();
        }

        public int GetSwearTimer()
        {
            if (config == null)
                return 1;
            return (int)config["SwearTimer"];
        }

        public string GetSwearString()
        {
            if (config == null)
                return "";
            return (string)config["SwearString"];
        }
    }
}
