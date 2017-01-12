using Discord;
using Maya.Chatterbot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Maya.Handlers
{
    public class BotHandler
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

        public async Task Initialize()
        {
            ready = false;
            ChatterBotFactory factory = new ChatterBotFactory();
            bot = factory.Create(ChatterBotType.CLEVERBOT);
            session = bot.CreateSession();
            await session.Initialize();
            ready = true;
        }

        public async Task<string> Think(string text)
        {
            return await session.Think(text);
        }

        public bool isReady()
        {
            return bot != null && session != null && ready;
        }

        public async Task Talk(IMessageChannel c, string text)
        {
            if (!isReady())
            {
                await c.SendMessageAsync("Loading...");
                return;
            }
            using (c.EnterTypingState())
            {
                await c.SendMessageAsync(await Think(text));
            }
        }
    }
}
