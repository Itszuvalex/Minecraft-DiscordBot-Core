using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using MinecraftDiscordBotCore.Models;
using MinecraftDiscordBotCore.Models.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinecraftDiscordBotCore.Services
{
    public class DiscordChatIntegrationService
    {
        private MinecraftServerHandler ServerHandler { get; }
        private DiscordSocketClient Discord { get; set; }
        private DataPersistenceService DataService { get; }
        public DiscordChatIntegrationService(MinecraftServerHandler serverHandler, DataPersistenceService dataService, DiscordSocketClient client)
        {
            ServerHandler = serverHandler;
            DataService = dataService;
            Discord = client;
        }

        public async Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            // Ignore system messages, or messages from other bots
            if (!(rawMessage is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;
            var context = new SocketCommandContext(Discord, message);
            var guild = context.Guild.Id;
            var channel = message.Channel.Id;
            var servers = DataService.ServerChatConnectionData.ServersForGuildChannel(guild, channel);
            foreach (var server in servers)
            {
                if (ServerHandler.TryGetServer(server, out MinecraftServer mcserver))
                {
                    await mcserver.SendMessage(new ChatMessage() { Message = string.Format("{0}/{1}/{2}: {3}", context.Guild.Name.Substring(0, 5), context.Channel.Name.Substring(0, 5), message.Author.Username, message.Content.Replace("\n", " ")), Timestamp = message.Timestamp.DateTime.ToString("HH:mm") });
                }
            }
        }

        public async Task MinecraftServerMessageReceivedAsync(MinecraftServer server, ChatMessage message)
        {
            var guildchannels = DataService.ServerChatConnectionData.GuildChannelsForServer(server.Guid);
            foreach(var guildchannel in guildchannels)
            {
                var channel = Discord.GetGuild(guildchannel.Guild).GetTextChannel(guildchannel.Channel);
                await channel.SendMessageAsync(string.Format("[{0}/{1}: {2}", server.Name, message.Timestamp, message.Message));
            }
        }
    }
}
