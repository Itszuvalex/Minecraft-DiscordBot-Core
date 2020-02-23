using MinecraftDiscordBotCore.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace MinecraftDiscordBotCore.Services
{
    public class DataPersistenceService
    {
        public BotControlData BotControlData { get; set; }
        public KnownServerData KnownServerData { get; set; }
        public ServerChatConnectionData ServerChatConnectionData { get; set; }
        public const string FolderName = "Data";

        public DataPersistenceService()
        {
            BotControlData = new BotControlData();
            KnownServerData = new KnownServerData();
            ServerChatConnectionData = new ServerChatConnectionData();
            if(!Directory.Exists(FolderName))
            {
                Directory.CreateDirectory(FolderName);
            }
            else
            {
                Load<BotControlData, Dictionary<string, HashSet<ulong>>>(BotControlData.DataKey, BotControlData);
                Load<KnownServerData, Dictionary<string, string>>(KnownServerData.DataKey, KnownServerData);
                Load<ServerChatConnectionData, Dictionary<string, List<GuildChannel>>>(ServerChatConnectionData.DataKey, ServerChatConnectionData);
            }
            BotControlData.DataPersistence = this;
            KnownServerData.DataPersistence = this;
            ServerChatConnectionData.DataPersistence = this;
        }


        public void Persist<T, Y>(string name, T o) where T : IPersistable<Y>
        {
            var persistable = o.GetPersistable();
            var json = JsonSerializer.Serialize(persistable, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FileForObj(name), json);
        }

        public void Load<T, Y>(string name, T o) where T: IPersistable<Y>
        {
            var file = FileForObj(name);
            if (!File.Exists(file))
                File.WriteAllText(file, "{}");
            var json = File.ReadAllText(file);
            var persistable = JsonSerializer.Deserialize<Y>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            o.FromPersistable(persistable);
        }

        private string FileForObj(string name)
        {
            return Path.Combine(FolderName, string.Format("{0}.json", name));
        }
    }
}
