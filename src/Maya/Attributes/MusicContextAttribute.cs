using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Maya.ModulesAddons;
using Maya.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Maya.Attributes
{
    public class MusicContextAttribute : PreconditionAttribute
    {
        public async override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IDependencyMap map)
        {
            if (!(context.Channel is ITextChannel))
                return PreconditionResult.FromError("null");
            if (!(context is MayaCommandContext))
                return PreconditionResult.FromError("null");
            MayaCommandContext con = context as MayaCommandContext;
            if (!con.MainHandler.GuildConfigHandler(con.Guild).getMusic().isEnabled)
                return PreconditionResult.FromError("Music system disabled.");
            if (!con.MainHandler.GuildMusicHandler(con.Guild).isReady())
                return PreconditionResult.FromError("Music system not ready.");
            MusicResult mr = con.MainHandler.GuildMusicHandler(con.Guild).canUserUseMusicCommands(new MusicContext(context));
            if (!mr.IsSuccessful)
                return PreconditionResult.FromError(mr.Error);
            IVoiceChannel vc = con.MainHandler.GuildMusicHandler(con.Guild).getVoiceChannel();
            IVoiceChannel channel = await Utils.findVoiceChannel(con.Guild as SocketGuild, con.MainHandler.GuildConfigHandler(con.Guild).getMusic().VoiceChannel);
            if (channel == null)
                return PreconditionResult.FromError($"Missing voice channel ({con.MainHandler.GuildConfigHandler(con.Guild).getMusic().VoiceChannel}).");
            else if(vc==null)
                await con.MainHandler.GuildMusicHandler(con.Guild).JoinVoiceChannel(channel);
            else if(vc != channel && !con.MainHandler.GuildMusicHandler(con.Guild).shouldDownload()) //!download = doing nothing
                await con.MainHandler.GuildMusicHandler(con.Guild).JoinVoiceChannel(channel);
            return PreconditionResult.FromSuccess();
        }
    }
}
