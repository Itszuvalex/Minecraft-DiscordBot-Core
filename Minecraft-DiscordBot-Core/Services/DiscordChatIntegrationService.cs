using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinecraftDiscordBotCore.Services
{
    public class DiscordChatIntegrationService
    {
        MinecraftServerHandler ServerHandler { get; }
        DiscordSocketClient Discord { get; }
        public DiscordChatIntegrationService(MinecraftServerHandler serverHandler, DiscordSocketClient discord)
        {
            ServerHandler = serverHandler;
            Discord = discord;
            Discord.MessageReceived += MessageReceivedAsync;
        }

        public async Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            // Ignore system messages, or messages from other bots
            if (!(rawMessage is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;
            var channel = rawMessage.Channel;

        }
    }
}
