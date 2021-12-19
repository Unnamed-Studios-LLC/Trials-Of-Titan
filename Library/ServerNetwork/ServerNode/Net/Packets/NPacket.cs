using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;
using Utils.NET.Net;

namespace ServerNode.Net.Packets
{
    public abstract class NPacket : Packet
    {
        public abstract NPacketType Type { get; }

        public override byte Id => (byte)Type;
    }
}
