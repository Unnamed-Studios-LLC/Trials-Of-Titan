using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;

namespace TitanCore.Net.Packets.Server
{
    public class TnDamageTaken : TnPacket
    {
        public override TnPacketType Type => TnPacketType.DamageTaken;

        public ushort damage;

        public TnDamageTaken()
        {

        }

        public TnDamageTaken(ushort damage)
        {
            this.damage = damage;
        }

        protected override void Read(BitReader r)
        {
            damage = r.ReadUInt16();
        }

        protected override void Write(BitWriter w)
        {
            w.Write(damage);
        }
    }
}
