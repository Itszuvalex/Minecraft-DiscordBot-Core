using MinecraftDiscordBotCore.Models.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace MinecraftDiscordBotCore.Models
{
    public class MinecraftServer
    {
        private WebSocket Socket { get; }
        private TaskCompletionSource<object> SocketFinishedTcs { get; }
        private McServerStatus Status;

        public MinecraftServer(WebSocket socket, TaskCompletionSource<object> socketFinishedTcs, McServerStatus initialStatus)
        {
            Socket = socket;
            SocketFinishedTcs = socketFinishedTcs;
            Status = initialStatus;
        }

        public async Task CloseAsync()
        {
            using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(2)); 
            await Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing connection", cancellationTokenSource.Token);
            SocketFinishedTcs?.SetResult(null);
        }
    }
}
