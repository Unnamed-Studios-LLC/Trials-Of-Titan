using System;
using System.Collections.Generic;
using System.Text;
using Test.Net.Packets;
using Utils.NET.Logging;
using Utils.NET.Net.Udp;

namespace Test.Net
{
    public class ClientTest : UdpClient<TestPacket>
    {

        protected override void HandleConnected(ConnectStatus status)
        {
            Log.Write("Client connect status: " + status);
        }

        protected override void HandleDisconnect()
        {
            Log.Write("Client disconnected");
        }

        protected override void HandlePacket(TestPacket packet)
        {
            Log.Write("Received packet: " + packet.Type);
        }
    }
}
