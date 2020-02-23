using MinecraftDiscordBotCore.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinecraftDiscordBotCore.Models
{
    public class ServerChatConnectionData : ServerToDiscordMapping
    {
        public override string DataKey => "ServerChatConnectionData";
    }
}
