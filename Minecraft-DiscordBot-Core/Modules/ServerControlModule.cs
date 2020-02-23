using Discord;
using Discord.Commands;
using MinecraftDiscordBotCore.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinecraftDiscordBotCore.Modules
{
    public class ServerControlModule : ModuleBase<SocketCommandContext>
    {
        public DataPersistenceService DataPersistence { get; set; }
        public MinecraftServerHandler ServerHandler { get; set; }

        [Command("las")]
        [Alias("listactiveservers", "lc", "activeservers", "connectedservers", "cs", "lcs", "listconnectedservers")]
        public Task ListActiveServers()
        {
            if (!DataPersistence.BotControlData.IsControlChannel(Context.Guild.Id, Context.Channel.Id))
                return Task.CompletedTask;

            var builder = new EmbedBuilder();
            builder.Description = "Connected servers";
            var servers = ServerHandler.AllServers();
            foreach(var server in servers)
            {
                builder.AddField(new EmbedFieldBuilder().WithValue(server.Name).WithName(server.Guid.ToString()));
            }
            return ReplyAsync(embed: builder.Build());
        }

        [Command("ls")]
        [Alias("las", "listknownservers", "listallsevers", "lks", "knownservers", "ks")]
        public Task ListKnownServers()
        {
            if (!DataPersistence.BotControlData.IsControlChannel(Context.Guild.Id, Context.Channel.Id))
                return Task.CompletedTask;

            var builder = new EmbedBuilder();
            builder.Description = "All servers";
            var servers = DataPersistence.KnownServerData.AllServers();
            foreach(var server in servers)
            {
                builder.AddField(new EmbedFieldBuilder().WithValue(server.Value).WithName(server.Key.ToString()));
            }
            return ReplyAsync(embed: builder.Build());
        }
    }
}
