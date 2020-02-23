using MinecraftDiscordBotCore.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinecraftDiscordBotCore.Models
{
    public class KnownServerData : IPersistable<Dictionary<string, string>>
    {
        public Dictionary<Guid, string> KnownServers;
        public DataPersistenceService DataPersistence { get; set; }
        public readonly string DataKey = "KnownServerData";

        public KnownServerData()
        {
            KnownServers = new Dictionary<Guid, string>();
        }

        public bool AddKnownServer(Guid guid, string name)
        {
            lock(KnownServers)
            {
                if(!KnownServers.TryGetValue(guid, out string oldname))
                {
                    KnownServers.Add(guid, name);
                    DataPersistence.Persist<KnownServerData, Dictionary<string, string>>(DataKey, this);
                    return true;
                }
                else if (!name.Equals(oldname))
                {
                    KnownServers[guid] = name;
                    DataPersistence.Persist<KnownServerData, Dictionary<string, string>>(DataKey, this);
                    return true;
                }
                return false;
            }
        }

        public bool RemoveKnownServer(Guid guid)
        {
            lock(KnownServers)
            {
                if(KnownServers.ContainsKey(guid))
                {
                    KnownServers.Remove(guid);
                    DataPersistence.Persist<KnownServerData, Dictionary<string, string>>(DataKey, this);
                    return true;
                }
                return false;
            }
        }

        public Dictionary<Guid, string> AllServers()
        {
            lock(KnownServers)
            {
                return new Dictionary<Guid, string>(KnownServers);
            }
        }

        public bool IsServerKnown(Guid guid, out string name)
        {
            lock(KnownServers)
            {
                return KnownServers.TryGetValue(guid, out name);
            }
        }

        public IEnumerable<Tuple<Guid, string>> KnownServersByName(string name)
        {
            var servers = new List<Tuple<Guid, string>>();
            lock (KnownServers)
            {
                foreach(var pair in KnownServers)
                {
                    if (pair.Value.Equals(name))
                        servers.Add(Tuple.Create(pair.Key, pair.Value));
                }
                return servers;
            }
        }

        public void FromPersistable(Dictionary<string, string> t)
        {
            lock (KnownServers)
            {
                KnownServers.Clear();
                foreach (var pair in t)
                {
                    KnownServers.Add(Guid.Parse(pair.Key), pair.Value);
                }
            }
        }

        public Dictionary<string, string> GetPersistable()
        {
            Dictionary<string, string> ret = new Dictionary<string, string>();
            lock (KnownServers)
            {
                foreach(var pair in KnownServers)
                {
                    ret.Add(pair.Key.ToString(), pair.Value);
                }
                return ret;
            }
        }
    }
}
