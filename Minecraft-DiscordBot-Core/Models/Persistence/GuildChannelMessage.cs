using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace MinecraftDiscordBotCore.Models
{
    public class GuildChannelMessage : IComparable<GuildChannelMessage>, IEquatable<GuildChannelMessage>
    {
        public GuildChannelMessage(ulong guild, ulong channel, ulong message)
        {
            Guild = guild;
            Channel = channel;
            Message = message;
        }

        public GuildChannelMessage() : this(0, 0, 0)
        { }

        public ulong Guild { get; set; }
        public ulong Channel { get; set; }
        public ulong Message { get; set; }

        public bool IsInChannel(GuildChannel channel)
        {
            return new GuildChannel(Guild, Channel) == channel;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (ReferenceEquals(obj, null))
            {
                return false;
            }

            if(!(obj is GuildChannelMessage gc))
                return false;

            return Guild == gc.Guild && Channel == gc.Channel && Message == gc.Message;
        }

        public override int GetHashCode()
        {
            return Guild.GetHashCode() ^ Channel.GetHashCode() ^ Message.GetHashCode();
        }

        public int CompareTo([AllowNull] GuildChannelMessage other)
        {
            if (other == null)
                return -1;

            if (Guild > other.Guild) return 1;
            if (Guild < other.Guild) return -1;
            if (Channel > other.Channel) return 1;
            if (Channel < other.Channel) return -1;
            if (Message > other.Message) return 1;
            if (Message < other.Message) return -1;

            return 0;
        }

        public bool Equals([AllowNull] GuildChannelMessage gc)
        {
            if (ReferenceEquals(this, gc))
            {
                return true;
            }

            if (ReferenceEquals(gc, null))
            {
                return false;
            }

            return Guild == gc.Guild && Channel == gc.Channel && Message == gc.Message;
        }

        public static bool operator ==(GuildChannelMessage left, GuildChannelMessage right)
        {
            if (ReferenceEquals(left, null))
            {
                return ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        public static bool operator !=(GuildChannelMessage left, GuildChannelMessage right)
        {
            return !(left == right);
        }

        public static bool operator <(GuildChannelMessage left, GuildChannelMessage right)
        {
            return ReferenceEquals(left, null) ? !ReferenceEquals(right, null) : left.CompareTo(right) < 0;
        }

        public static bool operator <=(GuildChannelMessage left, GuildChannelMessage right)
        {
            return ReferenceEquals(left, null) || left.CompareTo(right) <= 0;
        }

        public static bool operator >(GuildChannelMessage left, GuildChannelMessage right)
        {
            return !ReferenceEquals(left, null) && left.CompareTo(right) > 0;
        }

        public static bool operator >=(GuildChannelMessage left, GuildChannelMessage right)
        {
            return ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.CompareTo(right) >= 0;
        }
    }
}
