using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;
using Utils.NET.Net;

namespace TitanCore.Net.Packets
{
    public abstract class TnPacket : Packet
    {
        public override byte Id => (byte)Type;

        public abstract TnPacketType Type { get; }
    }
}
