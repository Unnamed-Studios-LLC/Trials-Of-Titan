using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;

namespace TitanCore.Net.Packets.Server
{
    public class TnDamageDealt : TnPacket
    {
        public override TnPacketType Type => TnPacketType.DamageDealt;

        public uint targetGameId;

        public ushort damage;

        public TnDamageDealt()
        {

        }

        public TnDamageDealt(uint targetGameId, ushort damage)
        {
            this.targetGameId = targetGameId;
            this.damage = damage;
        }

        protected override void Read(BitReader r)
        {
            targetGameId = r.ReadUInt32();
            damage = r.ReadUInt16();
        }

        protected override void Write(BitWriter w)
        {
            w.Write(targetGameId);
            w.Write(damage);
        }
    }
}
