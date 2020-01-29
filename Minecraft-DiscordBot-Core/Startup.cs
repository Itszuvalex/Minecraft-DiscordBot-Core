using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using MinecraftDiscordBotCore.Services;

namespace MinecraftDiscordBotCore
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            var webSocketOptions = new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120),
                ReceiveBufferSize = 4 * 1024
            };

            app.UseWebSockets(webSocketOptions);

            var connectionHandler = app.ApplicationServices.GetService(typeof(MinecraftServerConnectedHandler)) as MinecraftServerConnectedHandler;

            app.Use(async (context, next) =>
            {
                Console.WriteLine(String.Format("Received connection attempt at \"{0}\" with scheme {1}", context.Request.Path, context.Request.Scheme));
                if (context.Request.Path == "/ws")
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        var socketFinishedTcs = new TaskCompletionSource<object>();

                        connectionHandler.HandleWebsocket(webSocket, socketFinishedTcs);

                        await socketFinishedTcs.Task;
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                    }
                }
                else
                {
                    await next();
                }
            });
        }
    }
}
