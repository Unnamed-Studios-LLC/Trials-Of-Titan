using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;

namespace Utils.NET.Net.Udp.Packets
{
    public abstract class UdpPacket : Packet
    {
        public override byte Id => (byte)Type;

        public abstract UdpPacketType Type { get; }
    }
}
