using Discord;
using Maya.Chatterbot;
using Maya.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Maya.Handlers
{
    public class BotHandler : IHandler
    {
        private ChatterBot bot;
        private ChatterBotSession session;
        private bool ready;
        public BotHandler()
        {
            bot = null;
            session = null;
            ready = false;
        }

        public async Task InitializeAsync()
        {
            ready = false;
            ChatterBotFactory factory = new ChatterBotFactory();
            bot = factory.Create(ChatterBotType.CLEVERBOT);
            session = bot.CreateSession();
            await session.Initialize();
            ready = true;
        }

        public Task Close()
        {
            return Task.CompletedTask;
        }

        public async Task<string> ThinkAsync(string text)
        {
            return await session.Think(text);
        }

        public bool IsReady()
        {
            return bot != null && session != null && ready;
        }

        public async Task TalkAsync(IMessageChannel c, string text)
        {
            if (!IsReady())
            {
                await c.SendMessageAsync("Loading...");
                return;
            }
            using (c.EnterTypingState())
            {
                await c.SendMessageAsync(await ThinkAsync(text));
            }
        }
    }
}
