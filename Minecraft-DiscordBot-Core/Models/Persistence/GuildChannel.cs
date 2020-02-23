using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace MinecraftDiscordBotCore.Models
{
    public class GuildChannel : IComparable<GuildChannel>, IEquatable<GuildChannel>
    {
        public GuildChannel(ulong guild, ulong channel)
        {
            Guild = guild;
            Channel = channel;
        }

        public GuildChannel() : this(0, 0)
        { }

        public ulong Guild { get; set; }
        public ulong Channel { get; set; }

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

            if(!(obj is GuildChannel gc))
                return false;

            return Guild == gc.Guild && Channel == gc.Channel;
        }

        public override int GetHashCode()
        {
            return Guild.GetHashCode() ^ Channel.GetHashCode();
        }

        public int CompareTo([AllowNull] GuildChannel other)
        {
            if (other == null)
                return -1;

            if (Guild > other.Guild) return 1;
            if (Guild < other.Guild) return -1;
            if (Channel > other.Channel) return 1;
            if (Channel < other.Channel) return -1;

            return 0;
        }

        public bool Equals([AllowNull] GuildChannel gc)
        {
            if (ReferenceEquals(this, gc))
            {
                return true;
            }

            if (ReferenceEquals(gc, null))
            {
                return false;
            }

            return Guild == gc.Guild && Channel == gc.Channel;
        }

        public static bool operator ==(GuildChannel left, GuildChannel right)
        {
            if (ReferenceEquals(left, null))
            {
                return ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        public static bool operator !=(GuildChannel left, GuildChannel right)
        {
            return !(left == right);
        }

        public static bool operator <(GuildChannel left, GuildChannel right)
        {
            return ReferenceEquals(left, null) ? !ReferenceEquals(right, null) : left.CompareTo(right) < 0;
        }

        public static bool operator <=(GuildChannel left, GuildChannel right)
        {
            return ReferenceEquals(left, null) || left.CompareTo(right) <= 0;
        }

        public static bool operator >(GuildChannel left, GuildChannel right)
        {
            return !ReferenceEquals(left, null) && left.CompareTo(right) > 0;
        }

        public static bool operator >=(GuildChannel left, GuildChannel right)
        {
            return ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.CompareTo(right) >= 0;
        }
    }
}
