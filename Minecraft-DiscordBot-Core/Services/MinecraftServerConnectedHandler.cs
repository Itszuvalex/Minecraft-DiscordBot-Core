using System;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace MinecraftDiscordBotCore.Services {
    internal class MinecraftServerConnectedHandler
    {
        private MinecraftServerHandler ServerHandler { get; }
        public MinecraftServerConnectedHandler(MinecraftServerHandler handler)
        {
            ServerHandler = handler;
        }

        internal void HandleWebsocket(WebSocket webSocket, TaskCompletionSource<object> socketFinishedTcs)
        {
        }
    }
}