using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinecraftDiscordBotCore.Models
{
    public interface IPersistable<T>
    {
        T GetPersistable();
        void FromPersistable(T t);
    }
}
