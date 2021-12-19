using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;

namespace Test.Net.Packets
{
    public class A : TestPacket
    {
        public override TestPacketType Type => TestPacketType.A;

        protected override void Read(BitReader r)
        {

        }

        protected override void Write(BitWriter w)
        {

        }
    }
}
