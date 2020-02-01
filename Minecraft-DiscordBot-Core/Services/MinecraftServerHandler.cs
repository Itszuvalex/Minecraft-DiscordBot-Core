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
        }
    }
}
