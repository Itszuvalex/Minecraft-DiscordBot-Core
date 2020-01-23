using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinecraftDiscordBotCore.Models.Messages
{
    public class McServerStatus : IMessage
    {
        public const string TypeString = "status";
        public string Type => TypeString;

        public string Name { get; }
        public int Memory { get; }
        public int MemoryMax { get; }
        public ulong Storage { get; }
        public ulong StorageMax { get; }
        public string[] Players { get; }
        public int PlayerCount { get; }
        public int PlayerMax { get; }
        public IDictionary<int, float> Tps { get; }
        public string Status { get; }
        public int ActiveTime { get; }
    }
}
