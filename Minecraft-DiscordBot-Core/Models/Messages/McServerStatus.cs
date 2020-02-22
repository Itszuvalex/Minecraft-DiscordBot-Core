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

        public string Name { get; set; }
        public long Memory { get; set; }
        public long MemoryMax { get; set; }
        public long Storage { get; set; }
        public long StorageMax { get; set; }
        public string[] Players { get; set; }
        public int PlayerCount { get; set; }
        public int PlayerMax { get; set; }
        public Dictionary<string, float> Tps { get; set; }
        public string Status { get; set; }
        public int ActiveTime { get; set; }
    }
}
