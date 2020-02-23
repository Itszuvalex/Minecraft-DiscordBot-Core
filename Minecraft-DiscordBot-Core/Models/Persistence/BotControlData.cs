using MinecraftDiscordBotCore.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinecraftDiscordBotCore.Models
{
    public class BotControlData : IPersistable<Dictionary<string, HashSet<ulong>>>
    {
        public Dictionary<ulong, HashSet<ulong>> ControlChannels { get; set; }
        public DataPersistenceService DataPersistence { get; set; }
        public readonly string DataKey = "BotControlData";

        public BotControlData()
        {
            ControlChannels = new Dictionary<ulong, HashSet<ulong>>();
        }

        public bool IsControlChannel(ulong guild, ulong channel)
        {
            lock(ControlChannels)
            {
                if (!ControlChannels.TryGetValue(guild, out HashSet<ulong> channels))
                    return false;

                lock(channels)
                {
                    return channels.Contains(channel);
                }
            }
        }

        public bool AddControlChannel(ulong guild, ulong channel)
        {
            lock (ControlChannels)
            {
                HashSet<ulong> channels;
                if (!ControlChannels.TryGetValue(guild, out channels))
                {
                    channels = new HashSet<ulong>();
                    ControlChannels.Add(guild, channels);
                }

                lock (channels)
                {
                    if (channels.Contains(channel))
                        return false;

                    channels.Add(channel);
                }
                DataPersistence.Persist<BotControlData, Dictionary<string, HashSet<ulong>>>(DataKey, this);
            }

            return true;
        }

        public bool RemoveControlChannel(ulong guild, ulong channel)
        {
            lock (ControlChannels)
            {
                if (!ControlChannels.TryGetValue(guild, out HashSet<ulong> channels))
                    return false;

                lock (channels)
                {
                    if (!channels.Contains(channel))
                        return false;

                    channels.Remove(channel);
                    if (channels.Count == 0)
                    {
                        ControlChannels.Remove(guild);
                    }
                }
                DataPersistence.Persist<BotControlData, Dictionary<string, HashSet<ulong>>>(DataKey, this);
            }

            return true;
        }

        public Dictionary<string, HashSet<ulong>> GetPersistable()
        {
            lock (ControlChannels)
            {
                Dictionary<string, HashSet<ulong>> ret = new Dictionary<string, HashSet<ulong>>();
                foreach (var pair in ControlChannels)
                {
                    ret.Add(pair.Key.ToString(), pair.Value);
                }
                return ret;
            }
        }

        public void FromPersistable(Dictionary<string, HashSet<ulong>> t)
        {
            lock(ControlChannels)
            {
                ControlChannels.Clear();
                foreach(var pair in t)
                {
                    ControlChannels.Add(ulong.Parse(pair.Key), pair.Value);
                }
            }
        }
    }
}
