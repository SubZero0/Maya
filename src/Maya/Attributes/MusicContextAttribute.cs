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
        public async override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider map) //TODO: Remover connect aqui
        {
            if (!(context.Channel is ITextChannel))
                return PreconditionResult.FromError("null");
            if (!(context is MayaCommandContext))
                return PreconditionResult.FromError("null");
            MayaCommandContext con = context as MayaCommandContext;
            if (!con.MainHandler.GuildConfigHandler(con.Guild).GetMusic().IsEnabled)
                return PreconditionResult.FromError("Music system disabled.");
            if (!con.MainHandler.GuildMusicHandler(con.Guild).IsReady())
                return PreconditionResult.FromError("Music system not ready.");
            MusicResult mr = con.MainHandler.GuildMusicHandler(con.Guild).CanUserUseMusicCommands(new MusicContext(context));
            if (!mr.IsSuccessful)
                return PreconditionResult.FromError(mr.Error);
            IVoiceChannel vc = con.MainHandler.GuildMusicHandler(con.Guild).GetVoiceChannel();
            IVoiceChannel channel = Utils.FindVoiceChannel(con.Guild as SocketGuild, con.MainHandler.GuildConfigHandler(con.Guild).GetMusic().VoiceChannel);
            if (channel == null)
                return PreconditionResult.FromError($"Missing voice channel ({con.MainHandler.GuildConfigHandler(con.Guild).GetMusic().VoiceChannel}).");
            else if (vc == null)
                await con.MainHandler.GuildMusicHandler(con.Guild).JoinVoiceChannelAsync(channel);
            else if (vc != channel && !con.MainHandler.GuildMusicHandler(con.Guild).ShouldDownload()) //!download = doing nothing
                await con.MainHandler.GuildMusicHandler(con.Guild).JoinVoiceChannelAsync(channel);
            return PreconditionResult.FromSuccess();
        }
    }
}
