using Discord;
using Discord.Commands;
using MinecraftDiscordBotCore.Models;
using MinecraftDiscordBotCore.Models.Messages;
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
        public DiscordServerStatusMessageService StatusHandler { get; set; }

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
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(ChannelPermission.ManageChannels, ErrorMessage ="This requires channel management permissions.")]
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
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(ChannelPermission.ManageChannels, ErrorMessage ="This requires channel management permissions.")]
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

        [Command("AddStatusChannel")]
        [Alias("asc")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(ChannelPermission.ManageChannels, ErrorMessage ="This requires channel management permissions.")]
        public Task AddStatusChannel([Remainder] string text)
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

            if (!StatusHandler.AddStatusChannelForServer(new GuildChannel(Context.Guild.Id, Context.Channel.Id), server, serverName))
                return ReplyAsync("Server already has status message in this channel.");
            return Task.CompletedTask;
        }

        [Command("RemoveStatusChannel")]
        [Alias("rsc")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(ChannelPermission.ManageChannels, ErrorMessage ="This requires channel management permissions.")]
        public Task RemoveStatusChannel([Remainder] string text)
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

            if (!StatusHandler.RemoveStatusChannelForServer(new GuildChannel(Context.Guild.Id, Context.Channel.Id), server))
                return ReplyAsync("Server does not have status message in this channel.");
            return Task.CompletedTask;
        }

        [Command("SendServerCommand")]
        [Alias("cmd", "command")]
        [RequireUserPermission(ChannelPermission.ManageRoles, ErrorMessage ="This command is dangerous and requires ManageRoles permission since it's also dangerous")]
        public Task SendServerCommand([Remainder] string text)
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

            string restOfMessage = text.Trim();
            if(!string.IsNullOrEmpty(serverName))
            {
                if(restOfMessage.StartsWith("\""))
                {
                    var index = restOfMessage.Substring(1).IndexOf("\"");
                    restOfMessage = restOfMessage.Substring(index + 2);
                }
                else
                {
                    restOfMessage = restOfMessage.Substring(serverName.Length).Trim();
                }
            }
            else
            {
                restOfMessage = restOfMessage.Substring(server.ToString().Length);
            }
            
            if(!ServerHandler.TryGetServer(server, out MinecraftServer mcserver))
            {
                return ReplyAsync(string.Format("Failed to find connected server of guid={0}, name={1}", server, serverName));
            }

            return mcserver.SendMessage<ServerCommand>(new ServerCommand() { Command = restOfMessage.Replace("\n", " ") });
        }

        [Command("RemoveKnownServer")]
        [Alias("rks", "remove", "forget")]
        [RequireUserPermission(ChannelPermission.ManageRoles, ErrorMessage ="This command is dangerous and requires ManageRoles permission since it's also dangerous")]
        public Task RemoveKnownServer([Remainder] string text)
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

            if(ServerHandler.TryGetServer(server, out MinecraftServer mcserver))
            {
                return ReplyAsync("Can't forget a known server that's still connected.");
            }

            DataPersistence.KnownServerData.RemoveKnownServer(server);
            foreach(var guildchannel in DataPersistence.ServerChatConnectionData.GuildChannelsForServer(server))
            {
                DataPersistence.ServerChatConnectionData.RemoveGuildChannelForServer(guildchannel.Guild, guildchannel.Channel, server);
            }
            return ReplyAsync(string.Format("Forgot server name={0}, guid={1}", serverName, server));
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
                var trimmedstring = GetQuotedString(servernameguid);

                var servers = DataPersistence.KnownServerData.KnownServersByName(trimmedstring);
                if(servers.Count() == 0)
                {
                    error = String.Format("Could not find any server by name={0}", trimmedstring);
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

        private string GetQuotedString(string name)
        {
            var trimmedstring = name.Trim();
            if(name.StartsWith("\""))
            {
                var index = name.Substring(1).IndexOf("\"");
                if (index > 0)
                {
                    trimmedstring = name.Substring(1, index);
                }
            }
            return trimmedstring;
        }
    }
}
