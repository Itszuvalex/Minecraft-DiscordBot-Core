using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinecraftDiscordBotCore.Services
{
    public class BotTokenProviderService
    {
        public const string EnvironmentVariable = "token";

        public string _token = null;
        public string Token
        {
            get
            {
                if (string.IsNullOrEmpty(_token))
                {
                    string variable = Environment.GetEnvironmentVariable("token");
                    if (string.IsNullOrEmpty(variable))
                    {
                        variable = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Resources", "botkey.txt")).Trim();
                    }
                    _token = variable;
                }

                return _token;
            }
        }
    }
}
