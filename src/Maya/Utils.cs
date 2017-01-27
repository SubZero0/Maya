﻿using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Maya
{
    public class Utils
    {
        public static async Task SendMessageAsyncEx(string name, Func<Task<IUserMessage>> message)
        {
            if (message != null)
            {
                try
                {
                    await message.Invoke();
                }
                catch (Exception e)
                {
                    File.AppendAllText("exceptions.txt", $"Exception at {name}\n{e.ToString()}\n\n\n");
                    Console.WriteLine($">>EXCEPTION<< Exception at {name}!");
                }
            }
        }

        public static bool IsChannelListed(IChannel channel, List<string> list, bool allow_others = true)
        {
            if (!(channel is ITextChannel))
                return (allow_others ? true : false);
            if (list.Count == 0)
                return true;
            return list.Contains(channel.Name);
        }

        public async static Task<IVoiceChannel> FindVoiceChannel(SocketGuild guild, string voice_channel)
        {
            var cs = await guild.GetVoiceChannelsAsync();
            IVoiceChannel r = cs.FirstOrDefault(x => x.Name == voice_channel);
            if (r != null)
                return r;
            foreach (IVoiceChannel c in cs)
                if (c.Name.IndexOf(voice_channel, StringComparison.OrdinalIgnoreCase) != -1)
                    return c;
            return null;
        }

        public async static Task<ITextChannel> FindTextChannel(SocketGuild guild, string text_channel)
        {
            var cs = await guild.GetTextChannelsAsync();
            ITextChannel r = cs.FirstOrDefault(x => x.Name == text_channel);
            if (r != null)
                return r;
            foreach (ITextChannel c in cs)
                if (c.Name.IndexOf(text_channel, StringComparison.OrdinalIgnoreCase) != -1)
                    return c;
            return null;
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

        public static Color getRandomColor()
        {
            Random r = new Random();
            return new Color((byte)r.Next(255), (byte)r.Next(255), (byte)r.Next(255));
        }
        
        //Auto Delete Message
        public static async Task ADM(IUserMessage msg, int delay = 5000)
        {
            await Task.Delay(5000);
            await msg.DeleteAsync();
        }
    }
}
