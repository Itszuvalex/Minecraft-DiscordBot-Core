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
            Console.WriteLine("Received message in DIscordChatIntegrationService");
            Task sendTask = Task.Factory.StartNew(() =>
            {
                foreach (var server in servers)
                {
                    if (ServerHandler.TryGetServer(server, out MinecraftServer mcserver))
                    {
                        mcserver.SendMessage(new ChatMessage() { Message = string.Format("{0}: {1}", message.Author.Username, message.Content.Replace("\n", " ")), Timestamp = message.Timestamp.DateTime.ToString("HH:mm") }).Wait();
                    }
                }
            });
            await sendTask;
        }
    }
}
