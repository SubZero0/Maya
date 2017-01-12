using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Maya.Music
{
    public class MusicContext
    {
        public ITextChannel Channel { get; private set; }
        public IGuildUser AskedBy { get; private set; }
        public Song Song { get; private set; }
        public MusicContext(ITextChannel channel, IGuildUser user, Song music)
        {
            Channel = channel;
            AskedBy = user;
            Song = music;
        }
        public MusicContext(ICommandContext context)
        {
            Channel = context.Channel as ITextChannel;
            AskedBy = context.User as IGuildUser;
            Song = new Song();
        }
    }
}
