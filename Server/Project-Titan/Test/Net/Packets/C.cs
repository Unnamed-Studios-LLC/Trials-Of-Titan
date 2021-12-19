using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;

namespace Test.Net.Packets
{
    public class C : TestPacket
    {
        public override TestPacketType Type => TestPacketType.C;

        protected override void Read(BitReader r)
        {

        }

        protected override void Write(BitWriter w)
        {

        }
    }
}