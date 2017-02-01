using Discord;
using Discord.WebSocket;
using Maya.Interfaces;
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
    public class PersonalityHandler : IGuildHandler
    {
        private GuildHandler GuildHandler;
        private JObject personality;
        private Timer timer;
        private Dictionary<Regex, string> answers;
        private Nullable<int> lastHour;
        private TimeZoneInfo timezone;
        public PersonalityHandler(GuildHandler GuildHandler)
        {
            this.GuildHandler = GuildHandler;
            personality = null;
            timer = new Timer(TimerElapsed, null, Timeout.Infinite, Timeout.Infinite);
            answers = new Dictionary<Regex, string>();
            lastHour = null;
            var tzs = TimeZoneInfo.GetSystemTimeZones().Where(x => x.Id.Contains("Japan") || x.Id.Contains("Tokyo"));
            timezone = tzs.ElementAtOrDefault(0) ?? TimeZoneInfo.Utc;
        }

        public async Task InitializeAsync()
        {
            await LoadPersonalityAsync(GuildHandler.DatabaseHandler.GetPersonality());
        }

        public Task Close()
        {
            timer.Dispose();
            return Task.CompletedTask;
        }

        public bool ExistsPersonality(string name)
        {
            return File.Exists($"Configs{Path.DirectorySeparatorChar}Personalities{Path.DirectorySeparatorChar}{name.ToLower()}.json");
        }

        private async void TimerElapsed(object state)
        {
            DateTime dt = TimeZoneInfo.ConvertTime(DateTime.UtcNow, timezone);
            if (dt.Minute == 0 && (lastHour == null || (lastHour != null && lastHour.GetValueOrDefault() != dt.Hour)))
            {
                lastHour = dt.Hour;
                var ch = Utils.FindTextChannel(GuildHandler.Guild as SocketGuild, GuildHandler.ConfigHandler.GetNotifications().TextChannel);
                if (ch != null)
                    await GuildHandler.MainHandler.ExceptionHandler.SendMessageAsyncEx("PersonalityHourlyMessage", () => ch.SendMessageAsync((string)((JObject)personality["hourlyNotification"])[$"{lastHour = dt.Hour}"]));
            }
        }

        public async Task LoadPersonalityAsync(string name)
        {
            if (!ExistsPersonality(name))
                return;
            timer.Change(Timeout.Infinite, Timeout.Infinite);
            personality = null;
            answers.Clear();
            name = name.ToLower();
            personality = JObject.Parse(File.ReadAllText($"Configs{Path.DirectorySeparatorChar}Personalities{Path.DirectorySeparatorChar}{name}.json"));
            if (GuildHandler.ConfigHandler.GetNotifications().IsEnabled)
                if (personality["hourlyNotification"] != null)
                    timer.Change(6000, 60000);
            JObject ans = (JObject)personality["chatAnswers"];
            foreach (JProperty o in ans.Children())
            {
                Regex n = new Regex(o.Name, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                answers[n] = (string)ans[o.Name];
            }
            await (await GuildHandler.Guild.GetCurrentUserAsync()).ModifyAsync(x => x.Nickname = GetName());
        }

        public string GetName()
        {
            if (personality == null)
                return null;
            return (string)personality["name"];
        }

        public string GetAvatarUrl()
        {
            if (personality == null)
                return null;
            return (string)personality["avatar"];
        }

        public string GetOfflineString()
        {
            if (personality == null)
                return null;
            return (string)personality["offline"];
        }

        public string[,] GetOwnAnswers()
        {
            if (personality == null)
                return new string[,] { };
            return ((JArray)personality["ownAnswers"]).ToObject<string[,]>();
        }

        public string[,] GetMarryAnswers()
        {
            if (personality == null)
                return null;
            return ((JArray)personality["marryAnswers"]).ToObject<string[,]>();
        }

        public int GetChatInterferenceDelay()
        {
            if (personality == null)
                return 10;
            return (int)personality["chatInterferenceDelay"];
        }

        public string GetAnswer(string text)
        {
            if (personality == null)
                return null;
            foreach (Regex r in answers.Keys)
                if (r.Match(text).Success)
                    return answers[r];
            return null;
        }

        public bool IsReady()
        {
            return (personality != null);
        }
    }
}
