using MinecraftDiscordBotCore.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinecraftDiscordBotCore.Models
{
    public class ServerChatConnectionData : IPersistable<Dictionary<string, List<Tuple<ulong, ulong>>>>
    {
        private readonly object _lock = new object();
        private Dictionary<Guid, HashSet<Tuple<ulong, ulong>>> ServerToGuildChannelMap { get; }
        private Dictionary<Tuple<ulong, ulong>, HashSet<Guid>> GuildChannelToServerMap { get; }
        public DataPersistenceService DataPersistence { get; set; }
        public readonly string DataKey = "ServerChatConnectionData";
        public ServerChatConnectionData()
        {
            ServerToGuildChannelMap = new Dictionary<Guid, HashSet<Tuple<ulong, ulong>>>();
            GuildChannelToServerMap = new Dictionary<Tuple<ulong, ulong>, HashSet<Guid>>();
        }

        public bool AddGuildChannelForServer(ulong guild, ulong channel, Guid server)
        {
            lock(_lock)
            {
                var tuple = Tuple.Create(guild, channel);
                if (!TryAddServerGuildChannelToServerMap(tuple, server))
                    return false;

                if (!TryAddServerGuildChannelToGuildChannelMap(tuple, server))
                    return false;
            }
            DataPersistence.Persist<ServerChatConnectionData, Dictionary<string, List<Tuple<ulong, ulong>>>>(DataKey, this);
            return true;
        }

        private bool TryAddServerGuildChannelToServerMap(Tuple<ulong, ulong> tuple, Guid server)
        {
            HashSet<Tuple<ulong, ulong>> guildchannel;
            if(!ServerToGuildChannelMap.TryGetValue(server, out guildchannel))
            {
                guildchannel = new HashSet<Tuple<ulong, ulong>>();
                ServerToGuildChannelMap.Add(server, guildchannel);
            }
            
            lock(guildchannel)
            {
                if (guildchannel.Contains(tuple))
                    return false;
                guildchannel.Add(tuple);
            }
            return true;
        }

        private bool TryAddServerGuildChannelToGuildChannelMap(Tuple<ulong, ulong> tuple, Guid server)
        {
            HashSet<Guid> servers;
            if(!GuildChannelToServerMap.TryGetValue(tuple, out servers))
            {
                servers = new HashSet<Guid>();
                GuildChannelToServerMap.Add(tuple, servers);
            }

            lock(servers)
            {
                if (servers.Contains(server))
                    return false;
                servers.Add(server);
            }
            return true;
        }

        public bool RemoveGuildChannelForServer(ulong guild, ulong channel, Guid server)
        {
            lock(_lock)
            {

            }
            return true;
        }

        public void FromPersistable(Dictionary<string, List<Tuple<ulong, ulong>>> t)
        {
            lock(_lock)
            {
                GuildChannelToServerMap.Clear();
                ServerToGuildChannelMap.Clear();
                foreach(var pair in t)
                {
                    var guid = pair.Key;
                    var server = Guid.Parse(guid);
                    var guildchannels = pair.Value;
                    foreach (var gc in guildchannels)
                    {
                        TryAddServerGuildChannelToServerMap(gc, server);
                        TryAddServerGuildChannelToGuildChannelMap(gc, server);
                    }
                }
            }
        }

        public Dictionary<string, List<Tuple<ulong, ulong>>> GetPersistable()
        {
            Dictionary<string, List<Tuple<ulong, ulong>>> ret = new Dictionary<string, List<Tuple<ulong, ulong>>>();
            lock(_lock)
            {
                foreach(var pair in ServerToGuildChannelMap)
                {
                    var g = pair.Key.ToString();
                    var list = new List<Tuple<ulong, ulong>>();
                    foreach(var t in pair.Value)
                    {
                        list.Add(t);
                    }
                    ret.Add(g, list);
                }
            }
            return ret;
        }
    }
}
