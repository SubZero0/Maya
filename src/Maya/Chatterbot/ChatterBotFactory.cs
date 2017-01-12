/*
 * Code from chatter-bot-api
 * Credits to: pierredavidbelanger
 * Link: https://github.com/pierredavidbelanger/chatter-bot-api
 */
using System;

namespace Maya.Chatterbot
{
    public class ChatterBotFactory
    {
        public ChatterBot Create(ChatterBotType type)
        {
            return Create(type, null);
        }

        public ChatterBot Create(ChatterBotType type, object arg)
        {
            switch (type)
            {
                case ChatterBotType.CLEVERBOT:
                    return new Cleverbot("http://www.cleverbot.com/", "http://www.cleverbot.com/webservicemin?uc=3210&botapi=chatterbotapi", 26);
                case ChatterBotType.JABBERWACKY:
                    return new Cleverbot("http://jabberwacky.com", "http://jabberwacky.com/webservicemin?botapi=chatterbotapi", 20);
                case ChatterBotType.PANDORABOTS:
                    if (arg == null) throw new Exception("PANDORABOTS needs a botid arg");
                    return new Pandorabots(arg.ToString());
            }
            return null;
        }
    }
}
