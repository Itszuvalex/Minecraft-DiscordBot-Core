using MinecraftDiscordBotCore.Models;
using MinecraftDiscordBotCore.Models.Messages;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading;
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
            McServerStatus data; 
            try
            {
                if(!MinecraftServer.TryReceiveStatus(webSocket, out data))
                {
                    socketFinishedTcs.SetResult(false);
                    return;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(String.Format("Received exception while deserializing join status from server = {0}", e));
                socketFinishedTcs.SetException(e);
                return;
            }

            ServerHandler.AddServer(new MinecraftServer(webSocket, socketFinishedTcs, data));
        }
    }
}