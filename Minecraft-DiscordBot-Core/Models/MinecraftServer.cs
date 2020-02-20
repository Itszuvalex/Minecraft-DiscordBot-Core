using MinecraftDiscordBotCore.Models.Messages;
using MinecraftDiscordBotCore.Services;
using MinecraftDiscordBotCore.Utility;
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
        private CancellableRunLoop ReceiveLoop { get; }
        private MinecraftServerHandler ServerHandler { get; }
        private McServerStatus Status;

        public MinecraftServer(WebSocket socket, TaskCompletionSource<object> socketFinishedTcs, McServerStatus initialStatus, MinecraftServerHandler serverHandler)
        {
            Socket = socket;
            SocketFinishedTcs = socketFinishedTcs;
            Status = initialStatus;
            ReceiveLoop = new CancellableRunLoop();
            ReceiveLoop.LoopIterationEvent += ReceiveLoop_LoopIterationEvent;
            ServerHandler = serverHandler;
        }

        public void Listen()
        {
            Console.WriteLine("Starting to listen.");
            ReceiveLoop.Start();
        }

        private void ReceiveLoop_LoopIterationEvent(CancellationToken token)
        {
            Console.WriteLine("Starting to receive.");
            ArraySegment<byte> buffer = new byte[4 * 1024];
            using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(180));
            Task<WebSocketReceiveResult> dataTask = null;
            dataTask = Socket.ReceiveAsync(buffer, cancellationTokenSource.Token);
            try
            {
                dataTask.Wait();
            }
            catch (Exception e)
            {
                // Timed out
                Console.WriteLine(String.Format("Exception received when waiting on Socket ReceiveTask = {0}", e));
                ServerHandler.RemoveServer(this);
                return;
            }

            if (dataTask.IsCanceled || Socket.State != WebSocketState.Open)
            {
                // Timed out
                Console.WriteLine("Didn't receive a message from the server, timing out.");
                ServerHandler.RemoveServer(this);
                return;
            }

            OnDataReceived(buffer.Slice(0, dataTask.Result.Count));
        }

        private void OnDataReceived(ArraySegment<byte> data)
        {
            try
            {
                if (!IMessage.TryParseMessage<MessageHeader>(data, out MessageHeader message))
                {
                    Console.WriteLine("Failed to parse message header.");
                    return;
                }

                switch (message.Type)
                {
                    case ChatMessage.TypeString:
                        if (IMessage.TryParseMessage<ChatMessage>(data, out ChatMessage chatMessage))
                        {
                            Console.WriteLine(String.Format("Received Chat Message: {0}", chatMessage.Message));
                        }
                        else
                        {
                            Console.WriteLine(String.Format("Unable to parse ChatMessage out of ChatMessage header."));
                        }
                        break;
                    case McServerStatus.TypeString:
                        if (IMessage.TryParseMessage<McServerStatus>(data, out Status))
                        {
                            Console.WriteLine("Parsed ServerStatus.");
                        }
                        else
                        {
                            Console.WriteLine(String.Format("Unable to parse ServerStatus out of ServerStatus header."));
                        }
                        break;
                    default:
                        Console.WriteLine(String.Format("Unhandled Hub Message of type = {0}", message.Type));
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(String.Format("Error deserializing Json message = {0}", e.ToString()));
            }
        }

        public async Task CloseAsync()
        {
            if (ReceiveLoop.Running)
                ReceiveLoop.Stop();

            if (Socket.State == WebSocketState.Open || Socket.State == WebSocketState.Connecting)
            {
                using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                try
                {
                    await Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing connection", cancellationTokenSource.Token);
                }
                catch (Exception e)
                { }
            }
            SocketFinishedTcs?.SetResult(null);
        }

        public async Task SendMessage<T>(T message) where T : IMessage
        {
            if (Socket.State != WebSocketState.Open) return;
            byte[] json;
            try
            {
                json = JsonSerializer.SerializeToUtf8Bytes(message);
            }
            catch (Exception e)
            {
                Console.WriteLine(String.Format("Exception when serializing Json: {0}", e.ToString()));
                return;
            }

            var token = new CancellationToken();
            await Socket.SendAsync(json, WebSocketMessageType.Text, true, token);
        }

        public static bool TryReceiveStatus(WebSocket socket, out McServerStatus data)
        {
            ArraySegment<byte> buffer = new byte[4 * 1024];
            using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(180));
            Task<WebSocketReceiveResult> dataTask = null;
            dataTask = socket.ReceiveAsync(buffer, cancellationTokenSource.Token);
            try
            {
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

            return IMessage.TryParseMessage<McServerStatus>(buffer.Slice(0, dataTask.Result.Count), out data);
        }
    }
}
