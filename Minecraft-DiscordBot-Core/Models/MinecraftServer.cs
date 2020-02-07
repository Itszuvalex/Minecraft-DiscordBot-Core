using MinecraftDiscordBotCore.Models.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MinecraftDiscordBotCore.Models
{
    public class MinecraftServer
    {
        private WebSocket Socket { get; }
        private TaskCompletionSource<object> SocketFinishedTcs { get; }
        private CancellationTokenSource CancellationSource { get; set; }
        private Task ReceiveLoop { get; set; }
        private McServerStatus Status;

        public MinecraftServer(WebSocket socket, TaskCompletionSource<object> socketFinishedTcs, McServerStatus initialStatus)
        {
            Socket = socket;
            SocketFinishedTcs = socketFinishedTcs;
            Status = initialStatus;
        }

        public async Task ListenAsync()
        {
            CancellationSource = new CancellationTokenSource();
            ReceiveLoop = Task.Factory.StartNew(() =>
            {
                while(!CancellationSource.IsCancellationRequested)
                {
                    if (TryReceiveStatus(Socket, out Status))
                        Console.WriteLine(string.Format("Received Status from client = {0}", Status.ToString()));
                }
            }, TaskCreationOptions.LongRunning);
        }

        public async Task CloseAsync()
        {
            CancellationSource.Cancel();
            await ReceiveLoop;
            CancellationSource.Dispose();
            CancellationSource = null;
            using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(2)); 
            await Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing connection", cancellationTokenSource.Token);
            SocketFinishedTcs?.SetResult(null);
        }

        public static bool TryReceiveStatus(WebSocket socket, out McServerStatus data)
        {
            ArraySegment<byte> buffer = new byte[4 * 1024];
            using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(180));
            Task<WebSocketReceiveResult> dataTask = null;
            try
            {
                dataTask = socket.ReceiveAsync(buffer, cancellationTokenSource.Token);
                dataTask.Wait();
            }
            catch
            {
            }

            if (dataTask.IsCanceled || socket.State != WebSocketState.Open)
            {
                Console.WriteLine("Failed to receive status from websocket within 30 seconds.");
                data = null;
                return false;
            }

            Console.WriteLine(String.Format("Received message from client = {0}", Encoding.UTF8.GetString(buffer.Slice(0, dataTask.Result.Count))));

            data = JsonSerializer.Deserialize<McServerStatus>(buffer.Slice(0, dataTask.Result.Count), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return true;
        }
    }
}
