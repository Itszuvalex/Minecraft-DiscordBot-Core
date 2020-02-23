using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinecraftDiscordBotCore.Services
{
    public class DiscordServerStatusMessageService
    {
        private MinecraftServerHandler ServerHandler { get; }
        private DiscordSocketClient Discord { get; set; }
        private DataPersistenceService DataService { get; }
        public DiscordServerStatusMessageService(MinecraftServerHandler serverHandler, DataPersistenceService dataService, DiscordSocketClient client)
        {
            ServerHandler = serverHandler;
            DataService = dataService;
            Discord = client;
        }
    }
}
