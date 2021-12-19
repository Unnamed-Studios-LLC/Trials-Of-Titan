using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;

namespace TitanCore.Net.Packets.Models
{
    public struct ServerDamage
    {
        public uint gameId;

        public ushort damage;

        public ServerDamage(uint gameId, ushort damage)
        {
            this.gameId = gameId;
            this.damage = damage;
        }

        public ServerDamage(BitReader r)
        {
            gameId = r.ReadUInt32();
            damage = r.ReadUInt16();
        }

        public void Write(BitWriter w)
        {
            w.Write(gameId);
            w.Write(damage);
        }
    }
}
