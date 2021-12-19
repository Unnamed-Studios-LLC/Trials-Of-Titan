using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;
using Utils.NET.Net;

namespace World.Instances.Packets
{
    public abstract class InPacket : Packet
    {
        public abstract InPacketType Type { get; }

        public override byte Id => (byte)Type;

    }
}
