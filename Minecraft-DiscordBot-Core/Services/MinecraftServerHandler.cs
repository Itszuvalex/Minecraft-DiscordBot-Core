using MinecraftDiscordBotCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinecraftDiscordBotCore.Services
{
    public class MinecraftServerHandler
    {
        private List<MinecraftServer> Servers;

        public MinecraftServerHandler()
        {
            Servers = new List<MinecraftServer>();
        }

        public void AddServer(MinecraftServer server)
        {
            lock (Servers)
            {
                Servers.Add(server);
            }
            server.Listen();
            Console.WriteLine("Added server to list");
        }

        public void RemoveServer(MinecraftServer server)
        {
            Task close = server.CloseAsync();
            lock (Servers)
            {
                Servers.Remove(server);
            }
            close.Wait();
            Console.WriteLine("Removed server from list");
        }

        public void Close()
        {
            lock (Servers)
            {
                var closeTasks = Servers.Select((server) => server.CloseAsync());
                Task.WaitAll(closeTasks.ToArray());
                Servers.Clear();
            }
        }
    }
}
