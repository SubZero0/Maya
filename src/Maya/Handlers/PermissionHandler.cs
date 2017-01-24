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

        public async Task<bool> IsAdminAsync(IUser user)
        {
            return await Task.FromResult(false);
        }

        public async Task<bool> IsModAsync(IUser user)
        {
            return await Task.FromResult(false);
        }

        public async Task<bool> IsAtLeastAsync(IUser user, AdminLevel level)
        {
            if (await IsOwnerAsync(user) && AdminLevel.OWNER >= level)
                return await Task.FromResult(true);
            if (await IsAdminAsync(user) && AdminLevel.ADMIN >= level)
                return await Task.FromResult(true);
            if (await IsModAsync(user) && AdminLevel.MODERATOR >= level)
                return await Task.FromResult(true);
            if (AdminLevel.USER >= level)
                return await Task.FromResult(true);
            return false;
        }
    }
}
