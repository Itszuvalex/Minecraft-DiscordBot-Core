using Discord;
using Discord.Rest;
using Discord.WebSocket;
using MinecraftDiscordBotCore.Models;
using MinecraftDiscordBotCore.Models.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftDiscordBotCore.Services
{
    public class DiscordServerStatusMessageService
    {
        private MinecraftServerHandler ServerHandler { get; }
        private DiscordSocketClient Discord { get; set; }
        private DataPersistenceService DataService { get; }
        public DiscordServerStatusMessageService(DataPersistenceService dataService, DiscordSocketClient client)
        {
            DataService = dataService;
            Discord = client;
        }

        public bool AddStatusChannelForServer(GuildChannel guildchannel, Guid server, string name)
        {
            var messages = DataService.ServerStatusMessageData.GuildChannelMessagesForServer(server);
            if (messages.Any((msg) => msg.IsInChannel(guildchannel)))
                return false;
            var channel = Discord.GetGuild(guildchannel.Guild).GetTextChannel(guildchannel.Channel);
            var msg = channel.SendMessageAsync(embed: BuildEmbedForServer(server, name, null)).Result;
            _ = msg.PinAsync();
            var id = msg.Id;
            DataService.ServerStatusMessageData.AddGuildChannelMessageForServer(guildchannel.Guild, guildchannel.Channel, id, server);
            return true;
        }

        public bool RemoveStatusChannelForServer(GuildChannel guildchannel, Guid server)
        {
            var messages = DataService.ServerStatusMessageData.GuildChannelMessagesForServer(server).Where((m) => m.IsInChannel(guildchannel));
            if (!messages.Any())
                return false;

            foreach(var message in messages)
            {
                DataService.ServerStatusMessageData.RemoveGuildChannelMessageForServer(message.Guild, message.Channel, message.Message, server);
                var channel = Discord.GetGuild(message.Guild).GetTextChannel(message.Channel);
                _ = channel.GetMessageAsync(message.Message).ContinueWith((m) =>
                {
                    channel.DeleteMessageAsync(m.Result);
                });
            }
            return true;
        }

        public bool UpdateStatusMessageForServer(Guid guid, string name, MinecraftServer server)
        {
            var messages = DataService.ServerStatusMessageData.GuildChannelMessagesForServer(guid);
            var embed = BuildEmbedForServer(guid, name, server);
            foreach(var message in messages)
            {
                _ = Discord.GetGuild(message.Guild).GetTextChannel(message.Channel).GetMessageAsync(message.Message).ContinueWith((m) =>
                {
                    if(m.Result is RestUserMessage rm)
                    {
                        rm.ModifyAsync((mp) =>
                        {
                            mp.Embed = embed;
                        });
                    }
                });
            }
            return true;
        }

        public Embed BuildEmbedForServer(Guid guid, string name, MinecraftServer server)
        {
            var builder = new EmbedBuilder();
            builder.WithTitle(name);
            if(server == null)
            {
                builder.WithColor(0, 0, 0); // Black
                builder.AddField("Status", "Disconnected...");
            }
            else
            {
                var status = server.Status;
                var uptime = TimeSpan.FromSeconds(status.ActiveTime);
                var uptimestring = new StringBuilder();
                if (uptime.Days > 0)
                {
                    uptimestring.AppendFormat("{0:%d}", uptime).Append(" d, ");
                }
                if (uptime.Hours > 0)
                {
                    uptimestring.AppendFormat("{0:%h}", uptime).Append(" h, ");
                }
                if (uptime.Minutes > 0)
                {
                    uptimestring.AppendFormat("{0:%m}", uptime).Append(" m, ");
                }
                uptimestring.AppendFormat("{0:%s}", uptime).Append(" s");
                builder.AddField("Uptime", uptimestring.ToString());

                switch(status.Status)
                {
                    case "Stopped":
                        builder.WithColor(255, 255, 0); // Yellow
                        builder.AddField("Status", "Stopped...");
                        break;
                    case "Starting":
                        builder.WithColor(0, 0, 255); // Blue
                        builder.AddField("Status", "Starting...");
                        AddRunningMcServerStatus(builder, status);
                        break;
                    case "Running":
                        builder.WithColor(0, 255, 0); // Green
                        builder.AddField("Status", "Running...");
                        AddRunningMcServerStatus(builder, status);
                        break;
                    case "Error":
                        builder.WithColor(255, 0, 0); // Red
                        builder.AddField("Status", "Errored...");
                        break;
                    default:
                        builder.WithColor(100,100,100); // Grey
                        builder.AddField("Status", "Unsynced...");
                        break;
                }
            }
            builder.WithCurrentTimestamp();
            builder.WithFooter(new EmbedFooterBuilder().WithText(guid.ToString()));
            return builder.Build();
        }

        void AddRunningMcServerStatus(EmbedBuilder builder, McServerStatus status)
        {
            var mem = status.Memory;
            var memmax = status.MemoryMax;
            var sto = status.Storage;
            var stomax = status.StorageMax;
            var players = status.Players;
            var tps = status.Tps;
            builder.AddField("Memory", string.Format("({0:0.0}%) {1}/{2}", Convert.ToDouble(mem) / Convert.ToDouble(memmax), mem, memmax), true);
            builder.AddField("Storage", string.Format("({0:0.0}%) {1}/{2}", Convert.ToDouble(sto) / Convert.ToDouble(stomax), sto, stomax), true);
            var playerString = "No one online";
            if (players.Any())
                playerString = string.Join(" ", players);
            builder.AddField("Players", playerString);
            var tpsString = "No dimensions loaded";
            if (tps.Any())
                tpsString = string.Join(" ", tps.Select((a) => string.Format("{0}:{1:0.0}", a.Key, a.Value)));
            builder.AddField("Tps", tpsString);
        }
    }
}
