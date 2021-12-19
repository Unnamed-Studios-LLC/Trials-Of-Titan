using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;

namespace TitanCore.Net.Packets.Server
{
    public class TnQuestDamage : TnPacket
    {
        public override TnPacketType Type => TnPacketType.QuestDamage;

        public int damage;

        public TnQuestDamage()
        {

        }

        public TnQuestDamage(int damage)
        {
            this.damage = damage;
        }

        protected override void Read(BitReader r)
        {
            damage = r.ReadInt32();
        }

        protected override void Write(BitWriter w)
        {
            w.Write(damage);
        }
    }
}
