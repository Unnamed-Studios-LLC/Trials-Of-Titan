using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;

namespace Utils.NET.Net.Udp.Reliability
{
    public class UnreliableChannel<TPacket> : PacketChannel<TPacket> where TPacket : Packet
    {
        public override void ReceivePacket(BitReader r, byte packetId)
        {
            var packet = packetFactory.CreatePacket(packetId);
            packet.ReadPacket(r);
            doReceivePacket(packet);
        }

        public override void SendPacket(BitWriter w, TPacket packet)
        {
            packet.WritePacket(w);
            doSendPacket(w.GetData());
        }
    }
}
