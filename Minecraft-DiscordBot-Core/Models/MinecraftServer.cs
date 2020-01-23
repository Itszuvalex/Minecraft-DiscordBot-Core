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

        public MinecraftServer(WebSocket socket, TaskCompletionSource<object> socketFinishedTcs)
        {
            Socket = socket;
            SocketFinishedTcs = socketFinishedTcs;
        }

        public void Close()
        {
            SocketFinishedTcs?.SetResult(null);
        }
    }
}
