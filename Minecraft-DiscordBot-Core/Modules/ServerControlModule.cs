using Discord;
using Discord.Commands;
using MinecraftDiscordBotCore.Models;
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
        [Alias("listknownservers", "listallsevers", "lks", "knownservers", "ks")]
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

        [Command("AddChatChannel")]
        [Alias("acc")]
        public Task AddChatChannel([Remainder] string text)
        {
            if (!DataPersistence.BotControlData.IsControlChannel(Context.Guild.Id, Context.Channel.Id))
                return Task.CompletedTask;

            if(!TryGetServerByString(text, out Guid server, out string serverName, out string error, out Embed embederror))
            {
                if(embederror != null)
                {
                    return ReplyAsync(embed: embederror);
                }
                return ReplyAsync(error);
            }

            if (!DataPersistence.ServerChatConnectionData.AddGuildChannelForServer(Context.Guild.Id, Context.Channel.Id, server))
                return ReplyAsync("Chat channel already added or unsuccessful");
            return ReplyAsync("Successfully added chat channel");
        }

        [Command("RemoveChatChannel")]
        [Alias("rcc")]
        public Task RemoveChatChannel([Remainder] string text)
        {
            if (!DataPersistence.BotControlData.IsControlChannel(Context.Guild.Id, Context.Channel.Id))
                return Task.CompletedTask;

            if(!TryGetServerByString(text, out Guid server, out string serverName, out string error, out Embed embederror))
            {
                if(embederror != null)
                {
                    return ReplyAsync(embed: embederror);
                }
                return ReplyAsync(error);
            }

            if (!DataPersistence.ServerChatConnectionData.RemoveGuildChannelForServer(Context.Guild.Id, Context.Channel.Id, server))
                return ReplyAsync("Chat channel already removed or unsuccessful");
            return ReplyAsync("Successfully removed chat channel");
        }


        private bool TryGetServerByString(string id, out Guid server, out string name, out string error, out Embed embederror)
        {
            error = "";
            var servernameguid = id.Trim();
            embederror = null;
            server = Guid.Empty;
            name = "";
            if(Guid.TryParse(servernameguid, out Guid guid))
            {
                
                if(!DataPersistence.KnownServerData.IsServerKnown(guid, out name))
                {
                    error = String.Format("Could not find server by guid={0}", guid);
                    return false;
                }
            }
            else
            {
                var servers = DataPersistence.KnownServerData.KnownServersByName(servernameguid);
                if(servers.Count() == 0)
                {
                    error = String.Format("Could not find any server by name={0}", servernameguid);
                    return false;
                }
                else if (servers.Count() > 1)
                {
                    var builder = new EmbedBuilder();
                    builder.Title = "Found multiple matching servers";
                    builder.Description = "Matching servers";
                    foreach (var s in servers)
                    {
                        builder.AddField(new EmbedFieldBuilder().WithValue(s.Item2).WithName(s.Item1.ToString()));
                    }
                    embederror = builder.Build();
                    return false;
                }

                server = servers.First().Item1;
                name = servers.First().Item2;
            }
            return true;
        }
    }
}
