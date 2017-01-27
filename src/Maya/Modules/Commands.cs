using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Maya.Attributes;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json.Linq;
using System.IO;
using static Maya.Enums.Administration;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis;
using System.Reflection;
using Maya.Roslyn;
using Maya.WoWS;
using Maya.Handlers;
using System.Globalization;
using Maya.Music;
using Maya.ModulesAddons;
using Maya.GuildHandlers;

namespace Maya.Modules.Commands
{
    [Name("Player-specific")]
    public class PlayerCommands : ModuleBase<MayaCommandContext>
    {
        [Command("marie_splatoon"), Alias("marie")]
        [Summary("Special command for marie_splatoon")]
        public async Task Marie()
        {
            await ReplyAsync("Did you mean: boobs?  /人◕‿‿◕人\\");
        }

        [Command("khaenn"), Alias("khaen", "khaenn35")]
        [Summary("Special command for Khaenn35")]
        public async Task Khaen()
        {
            await ReplyAsync("http://i.imgur.com/zLNEnPy.png");
        }

        [Command("ktcraft"), Alias("kt", "serrith")]
        [Summary("Special command for KTcraft")]
        public async Task Kt()
        {
            await ReplyAsync("http://i.imgur.com/UeJ0dqC.png");
        }

        [Command("battleship_60"), Alias("bb60")]
        [Summary("Special command for Battleship_60")]
        public async Task Bb60()
        {
            await ReplyAsync("* spams chat internally *");
        }

        [Command("nightmare_fluttershy"), Alias("sparks")]
        [Summary("Special command for Nightmare_Fluttershy")]
        public async Task Sparks()
        {
            await ReplyAsync("http://i.imgur.com/fEM0XAm.png");
        }

        [Command("flieger56"), Alias("flieger")]
        [Summary("Special command for Flieger56")]
        public async Task Flieger()
        {
            await ReplyAsync("http://i.imgur.com/Pw2K8y4.png");
        }
    }

    [Name("Simple")]
    public class SimpleCommands : ModuleBase<MayaCommandContext>
    {
        [Command("hello"), Alias("hi", "hai")]
        [Summary("Says Hi")]
        public async Task Hi()
        {
            await ReplyAsync("Hi!");
        }

        [Command("onigiri")]
        [Summary("Show image of a onigiri")]
        public async Task Onigiri()
        {
            await ReplyAsync("", false, new EmbedBuilder().WithImageUrl("http://i.imgur.com/Rixlk2Y.png"));
        }

        [Command("wat")]
        [Summary("Show image of wat grandma")]
        public async Task Wat()
        {
            await ReplyAsync("", false, new EmbedBuilder().WithImageUrl("http://i.imgur.com/tAEmtB2.jpg"));
        }

        [Command("lewd")]
        [Summary("Show image of 'That's lewd'")]
        public async Task Lewd()
        {
            await ReplyAsync("", false, new EmbedBuilder().WithImageUrl("http://i.imgur.com/5VsznpP.png"));
        }

        [Command("wtf")]
        [Summary("Did you mean: wtr?")]
        public async Task Wtf()
        {
            await ReplyAsync("Did you mean: ?wtr?");
        }

        [Command("hug")]
        [Summary("Send a message saying * hugs *")]
        public async Task Hug()
        {
            await ReplyAsync($"* hugs {Context.User.Mention} * (✿◠‿◠)");
        }

        [Command("ping")]
        public async Task Ping()
        {
            await ReplyAsync($"🏓 Pong! ``{(Context.Client as DiscordSocketClient).Latency}ms``");
        }

        [Command("credits")]
        [Summary("Show the credits for creating the bot")]
        public async Task Credits()
        {
            var application = await Context.Client.GetApplicationInfoAsync();
            EmbedBuilder eb = new EmbedBuilder();
            IGuildUser bot = await Context.Guild.GetCurrentUserAsync();
            eb.Author = new EmbedAuthorBuilder().WithName("Credits").WithIconUrl(Context.Client.CurrentUser.AvatarUrl);
            eb.ThumbnailUrl = application.Owner.AvatarUrl;
            eb.Color = Maya.Utils.getRandomColor();
            eb.Description = $"Created by: {application.Owner.Mention}\n" +
                             $"Suggestions? Tell him. 😄\n\n" +
                              "Source: https://github.com/SubZero0/Maya";
            await ReplyAsync("", false, eb);
        }
    }

    [Name("Help")]
    public class HelpCommand : ModuleBase<MayaCommandContext>
    {
        [Command("help")]
        [Summary("Shows the help command")]
        public async Task Help([Remainder] string command = null)
        {
            if (Context.Channel is ITextChannel && !Context.MainHandler.GuildPersonalityHandler(Context.Guild).IsReady())
            {
                await ReplyAsync("Loading...");
                return;
            }
            await Context.MainHandler.CommandHandler.ShowHelpCommand(Context, command);
        }
    }
}
