using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinecraftDiscordBotCore.Models.Messages
{
    public class ServerId : IMessage
    {
        public const string TypeString = "id";
        public string Type => TypeString;
        public string Guid { get; set; }
        public string Name { get; set; }
    }
}
