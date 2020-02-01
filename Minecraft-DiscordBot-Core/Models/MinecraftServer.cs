using MinecraftDiscordBotCore.Models.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
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

        public void Close()
        {
            SocketFinishedTcs?.SetResult(null);
        }
    }
}
