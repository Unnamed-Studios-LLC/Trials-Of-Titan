using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Test.Net.Packets;
using Utils.NET.Logging;
using Utils.NET.Net.Udp;

namespace Test.Net
{
    public class ServerTest : UdpListener<ClientTest, TestPacket>
    {
        public ServerTest(int port, int maxClients) : base(port, maxClients)
        {
        }

        protected override void HandleConnection(ClientTest connection)
        {

        }

        protected override void HandleDisconnection(ClientTest connection)
        {
            throw new NotImplementedException();
        }
    }
}
