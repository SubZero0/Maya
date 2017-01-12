using Discord;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Maya.GuildHandlers
{
    public class PersonalityHandler
    {
        private GuildHandler GuildHandler;
        private JObject personality;
        private Timer timer;
        private Dictionary<Regex, string> answers;
        public PersonalityHandler(GuildHandler GuildHandler)
        {
            this.GuildHandler = GuildHandler;
            personality = null;
            timer = new Timer(Timer_Elapsed, null, Timeout.Infinite, Timeout.Infinite);
            answers = new Dictionary<Regex, string>();
        }

        public bool hasPersonality(string name)
        {
            return File.Exists($"Configs{Path.DirectorySeparatorChar}Personalities{Path.DirectorySeparatorChar}{name.ToLower()}.json");
        }

        public async Task Initialize()
        {
            await loadPersonality(GuildHandler.DatabaseHandler.getPersonality());
        }

        private async void Timer_Elapsed(object state)
        {
            try
            {
                DateTime dt;
                try { dt = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Japan")); }
                catch (Exception) { dt = DateTime.Now; }
                if (dt.Minute == 0)
                    (await Utils.findTextChannel(GuildHandler.Guild as SocketGuild, GuildHandler.ConfigHandler.getNotifications().TextChannel))?.SendMessageAsync((string)((JObject)personality["hourlyNotification"])[$"{dt.Hour}"]);
            }
            catch (Exception) { timer.Change(Timeout.Infinite, Timeout.Infinite); }
        }

        public async Task loadPersonality(string name)
        {
            if (!hasPersonality(name))
                return;
            timer.Change(Timeout.Infinite, Timeout.Infinite);
            personality = null;
            answers.Clear();
            name = name.ToLower();
            personality = JObject.Parse(File.ReadAllText($"Configs{Path.DirectorySeparatorChar}Personalities{Path.DirectorySeparatorChar}{name}.json"));
            await Task.Delay(500);
            if(GuildHandler.ConfigHandler.getNotifications().isEnabled)
                if (personality["hourlyNotification"] != null)
                    timer.Change(60000, 60000);
            JObject ans = (JObject)personality["chatAnswers"];
            foreach (JProperty o in ans.Children())
            {
                Regex n = new Regex(o.Name, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                answers[n] = (string)ans[o.Name];
            }
            await (await GuildHandler.Guild.GetCurrentUserAsync()).ModifyAsync(x => x.Nickname = getName());
        }

        public string getName()
        {
            if (personality == null)
                return null;
            return (string)personality["name"];
        }

        public string getAvatarUrl()
        {
            if (personality == null)
                return null;
            return (string)personality["avatar"];
        }

        public string getOfflineString()
        {
            if (personality == null)
                return null;
            return (string)personality["offline"];
        }

        public string[,] getOwnAnswers()
        {
            if (personality == null)
                return new string[,] { };
            return ((JArray)personality["ownAnswers"]).ToObject<string[,]>();
        }

        public string[,] getMarryAnswers()
        {
            if (personality == null)
                return null;
            return ((JArray)personality["marryAnswers"]).ToObject<string[,]>();
        }

        public int getChatInterferenceDelay()
        {
            if (personality == null)
                return 10;
            return (int)personality["chatInterferenceDelay"];
        }

        public string getAnswer(string text)
        {
            if (personality == null)
                return null;
            foreach (Regex r in answers.Keys)
                if (r.Match(text).Success)
                    return answers[r];
            return null;
        }

        public bool isReady()
        {
            return (personality != null);
        }
    }
}
