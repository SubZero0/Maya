using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Maya
{
    public static class Utils
    {
        public static bool IsChannelListed(this IChannel channel, List<string> list, bool allow_others = true)
        {
            if (!(channel is ITextChannel))
                return (allow_others ? true : false);
            if (list.Count == 0)
                return true;
            return list.Contains(channel.Name);
        }

        public static IVoiceChannel FindVoiceChannel(this SocketGuild guild, string voice_channel)
        {
            IVoiceChannel r = guild.VoiceChannels.FirstOrDefault(x => x.Name == voice_channel);
            if (r != null)
                return r;
            return guild.VoiceChannels.FirstOrDefault(x => (x.Name.IndexOf(voice_channel, StringComparison.OrdinalIgnoreCase)) != -1);
        }

        public static ITextChannel FindTextChannel(this SocketGuild guild, string text_channel)
        {
            ITextChannel r = guild.TextChannels.FirstOrDefault(x => x.Name == text_channel);
            if (r != null)
                return r;
            return guild.TextChannels.FirstOrDefault(x => (x.Name.IndexOf(text_channel, StringComparison.OrdinalIgnoreCase)) != -1);
        }

        public static SocketGuild FindGuild(this DiscordSocketClient discord, string guild_name)
        {
            var cs = discord.Guilds;
            SocketGuild r = cs.Where(x => x.Name == guild_name).FirstOrDefault();
            if (r != null)
                return r;
            foreach (SocketGuild c in cs)
                if (c.Name.IndexOf(guild_name, StringComparison.OrdinalIgnoreCase) != -1)
                    return c;
            return null;
        }

        public static string GetRandomWeightedChoice(string[,] arr)
        {
            Random r = new Random();
            int total = 0;
            for (int i = 0; i < arr.GetLength(0); i++)
                total += int.Parse(arr[i, 1]);
            String[] farr = new String[total];
            for (int i = 0; i < arr.GetLength(0); i++)
                for (int j = 0; j < int.Parse(arr[i, 1]); j++)
                {
                    int pos = r.Next(total);
                    bool insert = false;
                    while (!insert)
                    {
                        if (!String.IsNullOrEmpty(farr[pos]))
                        {
                            pos++;
                            if (pos == total)
                                pos = 0;
                        }
                        else
                        {
                            farr[pos] = arr[i, 0];
                            insert = true;
                        }
                    }
                }
            return farr[r.Next(total)];
        }

        public static string StripTags(string source)
        {
            char[] array = new char[source.Length];
            int arrayIndex = 0;
            bool inside = false;

            for (int i = 0; i < source.Length; i++)
            {
                char let = source[i];
                if (let == '<')
                {
                    inside = true;
                    continue;
                }
                if (let == '>')
                {
                    inside = false;
                    continue;
                }
                if (!inside)
                {
                    array[arrayIndex] = let;
                    arrayIndex++;
                }
            }
            return new string(array, 0, arrayIndex);
        }

        public static Color GetRandomColor()
        {
            Random r = new Random();
            return new Color((byte)r.Next(255), (byte)r.Next(255), (byte)r.Next(255));
        }

        public static string UnixTimestampToString(string unixTimestamp)
        {
            return DateTimeOffset.FromUnixTimeSeconds(long.Parse(unixTimestamp)).ToString("R", new CultureInfo("en-US"));
        }

        public static string Percentage(double v1, double v2)
        {
            if (v2 == 0)
                return "0%";
            return $"{(int)Math.Ceiling(v1 / v2 * 100)}%";
        }

        public static string ToReadableString(TimeSpan span)
        {
            return string.Join(", ", GetReadableStringElements(span).Where(str => !string.IsNullOrWhiteSpace(str)));
        }

        private static IEnumerable<string> GetReadableStringElements(TimeSpan span)
        {
            yield return GetHoursString(span.Hours);
            yield return GetMinutesString(span.Minutes);
            yield return GetSecondsString(span.Seconds);
        }

        private static string GetHoursString(int hours)
        {
            if (hours == 0)
                return string.Empty;
            if (hours == 1)
                return "1 hour";
            return string.Format("{0:0} hours", hours);
        }

        private static string GetMinutesString(int minutes)
        {
            if (minutes == 0)
                return string.Empty;
            if (minutes == 1)
                return "1 minute";
            return string.Format("{0:0} minutes", minutes);
        }

        private static string GetSecondsString(int seconds)
        {
            if (seconds == 0)
                return string.Empty;
            if (seconds == 1)
                return "1 second";
            return string.Format("{0:0} seconds", seconds);
        }

        public enum WoWSRegion
        {
            NA,
            EU,
            ASIA,
            RU,
            Unknown
        }

        public static WoWSRegion GetUserWoWSRegion(this IUser user)
        {
            if (user is IGuildUser)
                return GetUserWoWSRegion((IGuildUser)user);
            return WoWSRegion.Unknown;
        }
        public static WoWSRegion GetUserWoWSRegion(this IGuildUser user) => GetUserWoWSRegion(user as SocketGuildUser);
        public static WoWSRegion GetUserWoWSRegion(this SocketGuildUser user)
        {
            if (user == null)
                return WoWSRegion.Unknown;
            if (user.Roles.FirstOrDefault(x => x.Name == "NA") != null)
                return WoWSRegion.NA;
            if (user.Roles.FirstOrDefault(x => x.Name == "EU") != null)
                return WoWSRegion.EU;
            if (user.Roles.FirstOrDefault(x => x.Name == "ASIA") != null)
                return WoWSRegion.ASIA;
            if (user.Roles.FirstOrDefault(x => x.Name == "RU") != null)
                return WoWSRegion.RU;
            if (user.Roles.FirstOrDefault(x => x.Name == "CN") != null)
                return WoWSRegion.ASIA;
            return WoWSRegion.Unknown;
        }

        public static string GetWowsApiUrl(this WoWSRegion region)
        {
            switch(region)
            {
                case WoWSRegion.ASIA: { return "https://api.worldofwarships.asia/"; }
                case WoWSRegion.EU: { return "https://api.worldofwarships.eu/"; }
                case WoWSRegion.NA: { return "https://api.worldofwarships.com/"; }
                case WoWSRegion.RU: { return "https://api.worldofwarships.ru/"; }
                default: { return "https://api.worldofwarships.com/"; }
            }
        }

        public static string GetWtrUrl(this WoWSRegion region)
        {
            switch (region)
            {
                case WoWSRegion.ASIA: { return "http://asia.warshipstoday.com/"; }
                case WoWSRegion.EU: { return "http://eu.warshipstoday.com/"; }
                case WoWSRegion.NA: { return "http://na.warshipstoday.com/"; }
                case WoWSRegion.RU: { return "http://ru.warshipstoday.com/"; }
                default: { return "http://na.warshipstoday.com/"; }
            }
        }

        public static WoWSRegion StringToWowsRegion(string str)
        {
            if (str == null)
                return WoWSRegion.Unknown;
            str = str.ToLower();
            if (str == "na" || str == "north america")
                return WoWSRegion.NA;
            if (str == "eu" || str == "europe")
                return WoWSRegion.EU;
            if (str == "asia" || str == "cn" || str == "china")
                return WoWSRegion.ASIA;
            if (str == "ru" || str == "russia")
                return WoWSRegion.RU;
            return WoWSRegion.Unknown;
        }

        public static string WoWSRegionToString(this WoWSRegion region)
        {
            if (region == WoWSRegion.Unknown)
                return WoWSRegion.NA.ToString();
            return region.ToString();
        }
    }
}
