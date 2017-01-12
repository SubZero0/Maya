/*
 * Code from chatter-bot-api
 * Credits to: pierredavidbelanger
 * Link: https://github.com/pierredavidbelanger/chatter-bot-api
 */
using System.Threading.Tasks;

namespace Maya.Chatterbot
{
    public interface ChatterBotSession
    {
        Task<ChatterBotThought> Think(ChatterBotThought thought);
        Task<string> Think(string text);
        Task Initialize();
    }
}
