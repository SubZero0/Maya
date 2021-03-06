﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using System.Reflection;
using System.Text.RegularExpressions;
using Discord;
using Maya.Controllers;
using Maya.Interfaces;

namespace Maya.Handlers
{
    public class TextHandler : IHandler
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

        public async Task InitializeAsync(string swear_string)
        {
            swear = new Regex(swear_string, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            MainHandler.Client.MessageReceived += HandleTextAsync;
            await Task.CompletedTask;
        }

        public Task Close()
        {
            MainHandler.Client.MessageReceived -= HandleTextAsync;
            return Task.CompletedTask;
        }

        public void ResetInterferenceTime()
        {
            interferenceTime = null;
        }

        public async Task HandleTextAsync(SocketMessage parameterMessage)
        {
            var msg = parameterMessage as SocketUserMessage;
            if (msg == null) return;
            if (msg.Channel is ITextChannel && !MainHandler.GuildConfigHandler((msg.Channel as ITextChannel).Guild).IsChannelAllowed(msg.Channel)) return;

            if (msg.Channel is ITextChannel)
            {
                var channel = msg.Channel as ITextChannel;
                if (!MainHandler.GuildIgnoreHandler(channel.Guild).Contains(msg.Author.Id) && !msg.Author.IsBot)
                {
                    var autoResponse = MainHandler.GuildConfigHandler(channel.Guild).GetAutoResponse();
                    if (autoResponse.IsEnabled && channel.IsChannelListed(autoResponse.TextChannels))
                    {
                        var prefix = MainHandler.GetCommandPrefix(msg.Channel);
                        if (!(msg.Content.StartsWith(prefix) && msg.Content.Length > prefix.Length + 1 && char.IsLetter(msg.Content.ElementAt(prefix.Length + 1))))
                        {
                            bool interfere = true;
                            if (interferenceTime != null)
                                if (((DateTime.Now - interferenceTime.GetValueOrDefault())).Minutes < MainHandler.GuildPersonalityHandler(channel.Guild).GetChatInterferenceDelay())
                                    interfere = false;
                            if (interfere)
                            {
                                string ans = null;
                                /*if (Answers.isReady()) //TODO: Get answers file
                                    ans = Answers.getAnswer(e.Message.RawText);*/
                                if (MainHandler.GuildPersonalityHandler(channel.Guild).IsReady())
                                {
                                    string temp = MainHandler.GuildPersonalityHandler(channel.Guild).GetAnswer(msg.Content);
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
                    }
                    if (swear != null)
                    {
                        var sj = MainHandler.GuildConfigHandler(channel.Guild).GetSwearJar();
                        if (sj.IsEnabled && msg.Channel.IsChannelListed(sj.TextChannels))
                        {
                            bool swearjar = true;
                            if (swearTimer != null)
                                if (((TimeSpan)(DateTime.Now - swearTimer.GetValueOrDefault())).Minutes < MainHandler.ConfigHandler.GetSwearTimer())
                                    swearjar = false;
                            if (swearjar)
                                if (swear.Match(msg.Content).Success)
                                {
                                    swearTimer = DateTime.Now;
                                    MainHandler.GuildDatabaseHandler(channel.Guild).AddSwearJar(0.25);
                                    await msg.Channel.SendMessageAsync("More $0.25 to the swear jar!");
                                }
                        }
                    }
                    var chatterbot = MainHandler.GuildConfigHandler(channel.Guild).GetChatterBot();
                    if (chatterbot.IsEnabled && msg.Channel.IsChannelListed(chatterbot.TextChannels))
                    {
                        int argPos = 0;
                        if (msg.HasMentionPrefix(MainHandler.Client.CurrentUser, ref argPos))
                            await MainHandler.BotHandler.TalkAsync(msg.Channel, msg.Content.Substring(argPos));
                    }
                }
            }
            else
            {
                int argPos = 0;
                if (msg.HasMentionPrefix(MainHandler.Client.CurrentUser, ref argPos))
                    await MainHandler.BotHandler.TalkAsync(msg.Channel, msg.Content.Substring(argPos));
            }

            if (msg.Channel is ISocketPrivateChannel)
                if (await MainHandler.PermissionHandler.IsOwnerAsync(msg.Author))
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
                            SocketGuild g = MainHandler.Client.FindGuild(split[0]);
                            if (g == null)
                            {
                                await msg.Channel.SendMessageAsync("Guild not found");
                                return;
                            }
                            ITextChannel t = g.FindTextChannel(split[1]);
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
