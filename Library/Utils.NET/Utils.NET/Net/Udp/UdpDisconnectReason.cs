using System;
using System.Collections.Generic;
using System.Text;

namespace Utils.NET.Net.Udp
{
    public enum UdpDisconnectReason : byte
    {
        Unknown,
        Custom,
        ServerFull,
        ExistingConnection,
        ClientDisconnect,
        Timeout
    }
}
