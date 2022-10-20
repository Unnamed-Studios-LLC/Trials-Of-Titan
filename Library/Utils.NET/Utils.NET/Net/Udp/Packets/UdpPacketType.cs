using System;
using System.Collections.Generic;
using System.Text;

namespace Utils.NET.Net.Udp.Packets
{
    public enum UdpPacketType : byte
    {
        Connect,
        Challenge,
        Solution,
        Connected,
        Disconnect
    }
}
