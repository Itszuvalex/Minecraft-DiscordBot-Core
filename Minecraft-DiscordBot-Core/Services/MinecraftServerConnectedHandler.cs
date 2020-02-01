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
            ArraySegment<byte> buffer = new byte[4 * 1024];
            using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var dataTask = webSocket.ReceiveAsync(buffer, cancellationTokenSource.Token);
            dataTask.Wait();
            if (dataTask.IsCanceled)
            {
                Console.WriteLine("Failed to receive status from websocket within 30 seconds.");
                socketFinishedTcs.SetResult(false);
                return;
            }

            Console.WriteLine(String.Format("Received message from client = {0}", Encoding.UTF8.GetString(buffer.Slice(0, dataTask.Result.Count))));

            McServerStatus data; 
            try
            {
                data = JsonSerializer.Deserialize<McServerStatus>(buffer.Slice(0, dataTask.Result.Count), new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }catch (Exception e)
            {
                Console.WriteLine(String.Format("Received exception while deserializing join status from server = {0}", e));
                socketFinishedTcs.SetException(e);
                return;
            }

            ServerHandler.AddServer(new MinecraftServer(webSocket, socketFinishedTcs, data));
        }
    }
}