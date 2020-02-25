﻿using MinecraftDiscordBotCore.Models;
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
        private DataPersistenceService DataPersistence { get; }
        private DiscordChatIntegrationService Chat { get; }
        public MinecraftServerConnectedHandler(MinecraftServerHandler handler, DataPersistenceService data, DiscordChatIntegrationService chat)
        {
            ServerHandler = handler;
            DataPersistence = data;
            Chat = chat;
        }

        internal void HandleWebsocket(WebSocket webSocket, TaskCompletionSource<object> socketFinishedTcs)
        {
            ServerId id; 
            try
            {
                if(!MinecraftServer.TryReceiveId(webSocket, out id))
                {
                    socketFinishedTcs.SetResult(false);
                    return;
                }
                Console.WriteLine(String.Format("Received connection from server id={0}", id.Guid));
            }
            catch (Exception e)
            {
                Console.WriteLine(String.Format("Received exception while deserializing join status from server = {0}", e));
                socketFinishedTcs.SetException(e);
                return;
            }

            if(string.IsNullOrEmpty(id.Guid) || Guid.Parse(id.Guid) == Guid.Empty)
            {
                id.Guid = Guid.NewGuid().ToString();
                Console.WriteLine(String.Format("Id was unset, sending id to server: {0}", id.Guid));
                if (!MinecraftServer.TrySendId(webSocket, id))
                {
                    socketFinishedTcs.SetResult(false);
                    return;
                }
            }
            var server = new MinecraftServer(webSocket, socketFinishedTcs, ServerHandler, id, Chat);
            ServerHandler.AddServer(server);
            DataPersistence.KnownServerData.AddKnownServer(Guid.Parse(id.Guid), id.Name);
        }
    }
}