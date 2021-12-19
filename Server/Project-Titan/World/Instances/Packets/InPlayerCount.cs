using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;

namespace World.Instances.Packets
{
    public class InPlayerCount : InPacket
    {
        public override InPacketType Type => InPacketType.PlayerCount;

        public int count;

        public InPlayerCount()
        {

        }

        public InPlayerCount(int count)
        {
            this.count = count;
        }

        protected override void Read(BitReader r)
        {
            count = r.ReadInt32();
        }

        protected override void Write(BitWriter w)
        {
            w.Write(count);
        }
    }
}
