using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinecraftDiscordBotCore.Models.Messages
{
    public class MessageHeader : IMessage
    {
        public string Type { get; set; }
    }
}
