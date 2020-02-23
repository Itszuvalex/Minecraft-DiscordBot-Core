using MinecraftDiscordBotCore.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinecraftDiscordBotCore.Models
{
    public class ServerChatConnectionData : IPersistable<Dictionary<string, List<GuildChannel>>>
    {
        private readonly object _lock = new object();
        private Dictionary<Guid, HashSet<GuildChannel>> ServerToGuildChannelMap { get; }
        private Dictionary<GuildChannel, HashSet<Guid>> GuildChannelToServerMap { get; }
        public DataPersistenceService DataPersistence { get; set; }
        public readonly string DataKey = "ServerChatConnectionData";
        public ServerChatConnectionData()
        {
            ServerToGuildChannelMap = new Dictionary<Guid, HashSet<GuildChannel>>();
            GuildChannelToServerMap = new Dictionary<GuildChannel, HashSet<Guid>>();
        }

        public IEnumerable<GuildChannel> GuildChannelsForServer(Guid guid)
        {
            lock(_lock)
            {
                if(!ServerToGuildChannelMap.TryGetValue(guid, out HashSet<GuildChannel> guildchannels))
                {
                    return Enumerable.Empty<GuildChannel>();
                }
                return guildchannels.ToArray();
            }
        }

        public IEnumerable<Guid> ServersForGuildChannel(ulong guild, ulong channel)
        {
            var tuple = new GuildChannel(guild, channel);
            lock(_lock)
            {
                if(!GuildChannelToServerMap.TryGetValue(tuple, out HashSet<Guid> servers))
                {
                    return Enumerable.Empty<Guid>();
                }
                return servers.ToArray();
            }
        }

        public bool AddGuildChannelForServer(ulong guild, ulong channel, Guid server)
        {
            lock(_lock)
            {
                var tuple = new GuildChannel(guild, channel);
                if (!TryAddServerGuildChannelToServerMap(tuple, server))
                    return false;

                if (!TryAddServerGuildChannelToGuildChannelMap(tuple, server))
                    return false;
            }
            DataPersistence.Persist<ServerChatConnectionData, Dictionary<string, List<GuildChannel>>>(DataKey, this);
            return true;
        }

        private bool TryAddServerGuildChannelToServerMap(GuildChannel tuple, Guid server)
        {
            HashSet<GuildChannel> guildchannel;
            if(!ServerToGuildChannelMap.TryGetValue(server, out guildchannel))
            {
                guildchannel = new HashSet<GuildChannel>();
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

        private bool TryAddServerGuildChannelToGuildChannelMap(GuildChannel tuple, Guid server)
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
            var tuple = new GuildChannel(guild, channel);
            lock(_lock)
            {
                if (!ServerToGuildChannelMap.TryGetValue(server, out HashSet<GuildChannel> guildchannel))
                {
                    return false;
                }
                lock (guildchannel)
                {
                    guildchannel.Remove(tuple);
                    if(guildchannel.Count == 0)
                    {
                        ServerToGuildChannelMap.Remove(server);
                    }
                }

                if(!GuildChannelToServerMap.TryGetValue(tuple, out HashSet<Guid> servers))
                {
                    return false;
                }
                lock(servers)
                {
                    servers.Remove(server);
                    if(servers.Count == 0)
                    {
                        GuildChannelToServerMap.Remove(tuple);
                    }
                }
            }
            DataPersistence.Persist<ServerChatConnectionData, Dictionary<string, List<GuildChannel>>>(DataKey, this);
            return true;
        }

        public void FromPersistable(Dictionary<string, List<GuildChannel>> t)
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

        public Dictionary<string, List<GuildChannel>> GetPersistable()
        {
            Dictionary<string, List<GuildChannel>> ret = new Dictionary<string, List<GuildChannel>>();
            lock(_lock)
            {
                foreach(var pair in ServerToGuildChannelMap)
                {
                    var g = pair.Key.ToString();
                    var list = new List<GuildChannel>();
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
