using MinecraftDiscordBotCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinecraftDiscordBotCore.Services
{
    public class MinecraftServerHandler
    {
        private Dictionary<Guid, MinecraftServer> Servers;

        public MinecraftServerHandler()
        {
            Servers = new Dictionary<Guid, MinecraftServer>();
        }

        public IEnumerable<MinecraftServer> AllServers() { lock (Servers) { return Servers.Values.ToArray(); } }

        public void AddServer(MinecraftServer server)
        {
            lock (Servers)
            {
                Servers.Add(server.Guid, server);
            }
            server.Listen();
            Console.WriteLine("Added server to list");
        }

        public bool TryGetServer(Guid guid, out MinecraftServer server)
        {
            lock (Servers)
            {
                return Servers.TryGetValue(guid, out server);
            }
        }

        public IEnumerable<MinecraftServer> ServersByName(string name)
        {
            lock(Servers)
            {
                return Servers.Values.Where((a) => a.Name.Equals(name)).ToArray();
            }
        }

        public void RemoveServer(MinecraftServer server)
        {
            Task close = server.CloseAsync();
            lock (Servers)
            {
                Servers.Remove(server.Guid);
            }
            close.Wait();
            Console.WriteLine("Removed server from list");
        }

        public void Close()
        {
            lock (Servers)
            {
                var closeTasks = Servers.Select((server) => server.Value.CloseAsync());
                Task.WaitAll(closeTasks.ToArray());
                Servers.Clear();
            }
        }
    }
}
