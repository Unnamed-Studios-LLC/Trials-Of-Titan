using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;

namespace World.Instances.Packets
{
    public class InOverworldClosed : InPacket
    {
        public override InPacketType Type => InPacketType.OverworldClosed;

        protected override void Read(BitReader r)
        {

        }

        protected override void Write(BitWriter w)
        {

        }
    }
}
