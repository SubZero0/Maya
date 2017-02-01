using Discord;
using Maya.Controllers;
using Maya.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Maya.Handlers
{
    public class ExceptionHandler : IHandler
    {
        private MainHandler MainHandler;
        public ExceptionHandler(MainHandler MainHandler)
        {
            this.MainHandler = MainHandler;
        }

        public Task Close()
        {
            return Task.CompletedTask;
        }

        public async Task WriteException(Exception e)
        {
            StackTrace stackTrace = new StackTrace(e, true);
            await WriteException($"{stackTrace.GetFrames().ElementAt(0).GetMethod().DeclaringType?.Name} at {stackTrace.GetFrames().ElementAt(0).GetMethod().DeclaringType?.DeclaringType?.Name}", e);
        }
        public async Task WriteException(string place, Exception e)
        {
            await Task.Run(() => {
                File.AppendAllText("exceptions.txt", $"Exception at {place}\n{e.ToString()}\n\n\n");
                Console.WriteLine($"[ExceptionHandler] New exception at {place}.");
            });
        }

        public async Task SendMessageAsyncEx(string name, Func<Task<IUserMessage>> message)
        {
            if (message != null)
            {
                try
                {
                    await message.Invoke();
                }
                catch (Exception e)
                {
                    await WriteException(name, e);
                }
            }
        }
    }
}
