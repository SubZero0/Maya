using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Maya
{
    public class Utils
    {
        public static bool IsChannelListed(IChannel channel, List<string> list, bool allow_others = true)
        {
            if (!(channel is ITextChannel))
                return (allow_others ? true : false);
            if (list.Count == 0)
                return true;
            return list.Contains(channel.Name);
        }

        public static IVoiceChannel FindVoiceChannel(SocketGuild guild, string voice_channel)
        {
            IVoiceChannel r = guild.VoiceChannels.FirstOrDefault(x => x.Name == voice_channel);
            if (r != null)
                return r;
            return guild.VoiceChannels.FirstOrDefault(x => (x.Name.IndexOf(voice_channel, StringComparison.OrdinalIgnoreCase)) != -1);
        }

        public static ITextChannel FindTextChannel(SocketGuild guild, string text_channel)
        {
            ITextChannel r = guild.TextChannels.FirstOrDefault(x => x.Name == text_channel);
            if (r != null)
                return r;
            return guild.TextChannels.FirstOrDefault(x => (x.Name.IndexOf(text_channel, StringComparison.OrdinalIgnoreCase)) != -1);
        }

        public static SocketGuild FindGuild(DiscordSocketClient discord, string guild_name)
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
    }
}
