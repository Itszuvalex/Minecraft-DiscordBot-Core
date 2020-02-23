using Discord.Commands;
using MinecraftDiscordBotCore.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinecraftDiscordBotCore.Modules
{
    public class BotControlModule : ModuleBase<SocketCommandContext>
    {
        public DataPersistenceService DataPersistence { get; set; }

        [Command("AddCommandChannel")]
        [Alias("acc")]
        public Task AddCommandChannel()
        {
            bool success = DataPersistence.BotControlData.AddControlChannel(Context.Guild.Id, Context.Channel.Id);
            return ReplyAsync(success ? "Added command channel" : "Already a command channel");
        }

        [Command("RemoveCommandChannel")]
        [Alias("rcc")]
        public Task RemoveCommandChannel()
        {
            if (DataPersistence.BotControlData.IsControlChannel(Context.Guild.Id, Context.Channel.Id))
            {
                bool success = DataPersistence.BotControlData.RemoveControlChannel(Context.Guild.Id, Context.Channel.Id);
                return ReplyAsync(success ? "Removed command channel" : "Failed to remove, but is a control channel??");
            }
            return Task.CompletedTask;
        }
    }
}
