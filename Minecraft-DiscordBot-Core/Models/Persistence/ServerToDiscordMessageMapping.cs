using MinecraftDiscordBotCore.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinecraftDiscordBotCore.Models
{
    public abstract class ServerToDiscordMessageMapping : IPersistable<Dictionary<string, List<GuildChannelMessage>>>
    {
        private readonly object _lock = new object();
        private Dictionary<Guid, HashSet<GuildChannelMessage>> ServerToGuildChannelMessageMap { get; }
        private Dictionary<GuildChannelMessage, HashSet<Guid>> GuildChannelMessageToServerMap { get; }
        public DataPersistenceService DataPersistence { get; set; }
        public virtual string DataKey { get; }
        public ServerToDiscordMessageMapping()
        {
            ServerToGuildChannelMessageMap = new Dictionary<Guid, HashSet<GuildChannelMessage>>();
            GuildChannelMessageToServerMap = new Dictionary<GuildChannelMessage, HashSet<Guid>>();
        }

        public IEnumerable<GuildChannelMessage> GuildChannelMessagesForServer(Guid guid)
        {
            lock (_lock)
            {
                if (!ServerToGuildChannelMessageMap.TryGetValue(guid, out HashSet<GuildChannelMessage> guildchannels))
                {
                    return Enumerable.Empty<GuildChannelMessage>();
                }
                return guildchannels.ToArray();
            }
        }

        public IEnumerable<Guid> ServersForGuildChannelMessage(ulong guild, ulong channel, ulong message)
        {
            var tuple = new GuildChannelMessage(guild, channel, message);
            lock (_lock)
            {
                if (!GuildChannelMessageToServerMap.TryGetValue(tuple, out HashSet<Guid> servers))
                {
                    return Enumerable.Empty<Guid>();
                }
                return servers.ToArray();
            }
        }

        public bool AddGuildChannelMessageForServer(ulong guild, ulong channel, ulong message, Guid server)
        {
            lock (_lock)
            {
                var tuple = new GuildChannelMessage(guild, channel, message);
                if (!TryAddServerGuildChannelMessageToServerMap(tuple, server))
                    return false;

                if (!TryAddServerGuildChannelMessageToGuildChannelMessageMap(tuple, server))
                    return false;
            }
            DataPersistence.Persist<ServerToDiscordMessageMapping, Dictionary<string, List<GuildChannelMessage>>>(DataKey, this);
            return true;
        }

        private bool TryAddServerGuildChannelMessageToServerMap(GuildChannelMessage tuple, Guid server)
        {
            HashSet<GuildChannelMessage> guildchannel;
            if (!ServerToGuildChannelMessageMap.TryGetValue(server, out guildchannel))
            {
                guildchannel = new HashSet<GuildChannelMessage>();
                ServerToGuildChannelMessageMap.Add(server, guildchannel);
            }

            lock (guildchannel)
            {
                if (guildchannel.Contains(tuple))
                    return false;
                guildchannel.Add(tuple);
            }
            return true;
        }

        private bool TryAddServerGuildChannelMessageToGuildChannelMessageMap(GuildChannelMessage tuple, Guid server)
        {
            HashSet<Guid> servers;
            if (!GuildChannelMessageToServerMap.TryGetValue(tuple, out servers))
            {
                servers = new HashSet<Guid>();
                GuildChannelMessageToServerMap.Add(tuple, servers);
            }

            lock (servers)
            {
                if (servers.Contains(server))
                    return false;
                servers.Add(server);
            }
            return true;
        }

        public bool RemoveGuildChannelMessageForServer(ulong guild, ulong channel, ulong message, Guid server)
        {
            var tuple = new GuildChannelMessage(guild, channel, message);
            lock (_lock)
            {
                if (!ServerToGuildChannelMessageMap.TryGetValue(server, out HashSet<GuildChannelMessage> guildchannel))
                {
                    return false;
                }
                lock (guildchannel)
                {
                    guildchannel.Remove(tuple);
                    if (guildchannel.Count == 0)
                    {
                        ServerToGuildChannelMessageMap.Remove(server);
                    }
                }

                if (!GuildChannelMessageToServerMap.TryGetValue(tuple, out HashSet<Guid> servers))
                {
                    return false;
                }
                lock (servers)
                {
                    servers.Remove(server);
                    if (servers.Count == 0)
                    {
                        GuildChannelMessageToServerMap.Remove(tuple);
                    }
                }
            }
            DataPersistence.Persist<ServerToDiscordMessageMapping, Dictionary<string, List<GuildChannelMessage>>>(DataKey, this);
            return true;
        }

        public void FromPersistable(Dictionary<string, List<GuildChannelMessage>> t)
        {
            lock (_lock)
            {
                GuildChannelMessageToServerMap.Clear();
                ServerToGuildChannelMessageMap.Clear();
                foreach (var pair in t)
                {
                    var guid = pair.Key;
                    var server = Guid.Parse(guid);
                    var guildchannels = pair.Value;
                    foreach (var gc in guildchannels)
                    {
                        TryAddServerGuildChannelMessageToServerMap(gc, server);
                        TryAddServerGuildChannelMessageToGuildChannelMessageMap(gc, server);
                    }
                }
            }
        }

        public Dictionary<string, List<GuildChannelMessage>> GetPersistable()
        {
            Dictionary<string, List<GuildChannelMessage>> ret = new Dictionary<string, List<GuildChannelMessage>>();
            lock (_lock)
            {
                foreach (var pair in ServerToGuildChannelMessageMap)
                {
                    var g = pair.Key.ToString();
                    var list = new List<GuildChannelMessage>();
                    foreach (var t in pair.Value)
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
