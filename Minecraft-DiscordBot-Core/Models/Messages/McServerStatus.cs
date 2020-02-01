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
        public long Memory { get; }
        public long MemoryMax { get; }
        public long Storage { get; }
        public long StorageMax { get; }
        public string[] Players { get; }
        public int PlayerCount { get; }
        public int PlayerMax { get; }
        public Dictionary<string, float> Tps { get; }
        public string Status { get; }
        public int ActiveTime { get; }
    }
}
