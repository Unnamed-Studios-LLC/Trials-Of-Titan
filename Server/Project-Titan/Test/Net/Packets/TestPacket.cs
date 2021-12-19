using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;
using Utils.NET.Net;

namespace Test.Net.Packets
{
    public abstract class TestPacket : Packet
    {
        public override byte Id => (byte)Type;

        public abstract TestPacketType Type { get; }
    }

    public enum TestPacketType : byte
    {
        A = 0,
        B = 1,
        C = 2
    }
}
