using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using System.Reflection;
using System.Text.RegularExpressions;
using Discord;
using Maya.Controllers;

namespace Maya
{
    public class TextHandler
    {
        private MainHandler MainHandler;
        private Nullable<DateTime> interferenceTime;
        private Regex swear;
        private Nullable<DateTime> swearTimer;
        private ITextChannel focus;
        public TextHandler(MainHandler MainHandler)
        {
            this.MainHandler = MainHandler;
            interferenceTime = swearTimer = null;
            swear = null;
            focus = null;
        }

        public void Initialize(string swear_string)
        {
            swear = new Regex(swear_string, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            MainHandler.Client.MessageReceived += HandleText;
        }

        public void resetInterferenceTime()
        {
            interferenceTime = null;
        }

        public async Task HandleText(SocketMessage parameterMessage)
        {
            var msg = parameterMessage as SocketUserMessage;
            if (msg == null) return;
            if (msg.Channel is ITextChannel && !MainHandler.GuildConfigHandler((msg.Channel as ITextChannel).Guild).isChannelAllowed(msg.Channel)) return;

            if (msg.Channel is ITextChannel)
            {
                var channel = msg.Channel as ITextChannel;
                if (!MainHandler.GuildIgnoreHandler(channel.Guild).contains(msg.Author.Id) && !msg.Author.IsBot)
                {
                    if (!(msg.Content.StartsWith(MainHandler.getCommandPrefix(msg.Channel)) && char.IsLetter(msg.Content.ElementAt(1))))
                    {
                        bool interfere = true;
                        if (interferenceTime != null)
                            if (((TimeSpan)(DateTime.Now - interferenceTime.GetValueOrDefault())).Minutes < MainHandler.GuildPersonalityHandler(channel.Guild).getChatInterferenceDelay())
                                interfere = false;
                        if (interfere)
                        {
                            string ans = null;
                            /*if (Answers.isReady()) //TODO: Get answers file!!
                                ans = Answers.getAnswer(e.Message.RawText);*/
                            if (MainHandler.GuildPersonalityHandler(channel.Guild).isReady())
                            {
                                string temp = MainHandler.GuildPersonalityHandler(channel.Guild).getAnswer(msg.Content);
                                if (temp != null)
                                    ans = temp;
                            }
                            if (ans != null)
                            {
                                interferenceTime = DateTime.Now;
                                await msg.Channel.SendMessageAsync(ans);
                            }
                        }
                    }
                    if (MainHandler.GuildPersonalityHandler(channel.Guild).isReady())
                        if (msg.Content.IndexOf($"{MainHandler.getCommandPrefix(channel)}{MainHandler.GuildPersonalityHandler(channel.Guild).getName()}", StringComparison.OrdinalIgnoreCase) != -1)
                        {
                            string[,] own = MainHandler.GuildPersonalityHandler(channel.Guild).getOwnAnswers();
                            string[,] temp = MainHandler.ConfigHandler.getAnswers();
                            string[,] sum = new string[own.GetLength(0) + temp.GetLength(0), 2];
                            for (int i = 0; i < own.GetLength(0); i++)
                            {
                                sum[i, 0] = own[i, 0];
                                sum[i, 1] = own[i, 1];
                            }
                            for (int i = 0; i < temp.GetLength(0); i++)
                            {
                                sum[own.GetLength(0) + i, 0] = temp[i, 0];
                                sum[own.GetLength(0) + i, 1] = temp[i, 1];
                            }
                            await msg.Channel.SendMessageAsync(Utils.getRandomWeightedChoice(sum));
                            return;
                        }
                    if (swear != null)
                    {
                        var sj = MainHandler.GuildConfigHandler(channel.Guild).getSwearJar();
                        if (sj.isEnabled && Utils.isChannelListed(msg.Channel, sj.TextChannels))
                        {
                            bool swearjar = true;
                            if (swearTimer != null)
                                if (((TimeSpan)(DateTime.Now - swearTimer.GetValueOrDefault())).Minutes < MainHandler.ConfigHandler.getSwearTimer())
                                    swearjar = false;
                            if (swearjar)
                                if (swear.Match(msg.Content).Success)
                                {
                                    swearTimer = DateTime.Now;
                                    MainHandler.GuildDatabaseHandler(channel.Guild).addSwearJar(0.25);
                                    await msg.Channel.SendMessageAsync("More $0.25 to the swear jar!");
                                }
                        }
                    }
                    var chatterbot = MainHandler.GuildConfigHandler(channel.Guild).getChatterBot();
                    if (chatterbot.isEnabled && Utils.isChannelListed(msg.Channel, chatterbot.TextChannels))
                    {
                        int argPos = 0;
                        if (msg.HasMentionPrefix(MainHandler.Client.CurrentUser, ref argPos))
                            await MainHandler.BotHandler.Talk(msg.Channel, msg.Content.Substring(argPos));
                    }
                }
            }
            else
            {
                int argPos = 0;
                if (msg.HasMentionPrefix(MainHandler.Client.CurrentUser, ref argPos))
                    await MainHandler.BotHandler.Talk(msg.Channel, msg.Content.Substring(argPos));
            }

            if (msg.Channel is ISocketPrivateChannel)
                if (await MainHandler.PermissionHandler.isOwner(msg.Author))
                {
                    if (msg.Content.StartsWith("focus"))
                    {
                        if (msg.Content.Count() == 5)
                        {
                            await msg.Channel.SendMessageAsync("focus Guild Name | Channel name\nfocus null");
                            return;
                        }
                        string str = msg.Content.Substring(6);
                        if (str.Equals("null"))
                        {
                            focus = null;
                            await msg.Channel.SendMessageAsync("Stopped focusing");
                        }
                        else
                        {
                            if (!str.Contains(" | "))
                            {
                                await msg.Channel.SendMessageAsync("focus Guild Name | Channel name");
                                return;
                            }
                            string[] split = str.Split(new string[] { " | " }, StringSplitOptions.None);
                            SocketGuild g = Utils.findGuild(MainHandler.Client, split[0]);
                            if (g == null)
                            {
                                await msg.Channel.SendMessageAsync("Guild not found");
                                return;
                            }
                            ITextChannel t = await Utils.findTextChannel(g, split[1]);
                            if (t == null)
                            {
                                await msg.Channel.SendMessageAsync("Text channel not found");
                                return;
                            }
                            await msg.Channel.SendMessageAsync($"Focusing at {g.Name}/{t.Name}");
                            focus = t;
                        }
                    }
                    else if (focus != null)
                    {
                        try
                        {
                            await focus.SendMessageAsync(msg.Content);
                        }
                        catch (Exception) { }
                    }
                }
        }
    }
}
