using Discord;
using Maya.Controllers;
using Maya.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Maya.Enums.Administration;

namespace Maya.Handlers
{
    public class PermissionHandler : IHandler
    {
        private MainHandler MainHandler;
        public PermissionHandler(MainHandler MainHandler)
        {
            this.MainHandler = MainHandler;
        }

        public Task Close()
        {
            return Task.CompletedTask;
        }

        public async Task<bool> IsOwnerAsync(IUser user)
        {
            return user.Id == (await MainHandler.Client.GetApplicationInfoAsync()).Owner.Id;
        }

        public Task<bool> IsAdminAsync(IUser user)
        {
            return Task.FromResult(false);
        }

        public Task<bool> IsModAsync(IUser user)
        {
            if (user is IGuildUser)
                return Task.FromResult(((IGuildUser)user).GuildPermissions.Administrator);
            return Task.FromResult(false);
        }

        public async Task<bool> IsAtLeastAsync(IUser user, AdminLevel level)
        {
            return await GetAdminLevelAsync(user) >= level;
        }

        public async Task<AdminLevel> GetAdminLevelAsync(IUser user)
        {
            if (await IsOwnerAsync(user))
                return AdminLevel.OWNER;
            else if (await IsAdminAsync(user))
                return AdminLevel.ADMIN;
            else if (await IsModAsync(user))
                return AdminLevel.MODERATOR;
            else
                return AdminLevel.USER;
        }
    }
}
