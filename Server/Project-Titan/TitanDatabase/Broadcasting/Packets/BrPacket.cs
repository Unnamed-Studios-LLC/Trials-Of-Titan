using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;
using Utils.NET.Net;

namespace TitanDatabase.Broadcasting.Packets
{
    public abstract class BrPacket : Packet
    {
        public abstract BrPacketType Type { get; }

        public override byte Id => (byte)Type;
    }
}
