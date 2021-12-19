using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Net.Packets.Client;
using TitanCore.Net.Packets.Server;

namespace World.Net.Handling
{
    public class PingHandler : ClientPacketHandler<TnPing>
    {
        public override void Handle(TnPing packet, Client connection)
        {
            connection.SendAsync(new TnPong(0));
        }
    }
}
