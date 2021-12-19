using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Net.Packets.Client;

namespace World.Net.Handling
{
    public class PongHandler : ClientPacketHandler<TnPong>
    {
        public override void Handle(TnPong packet, Client connection)
        {
            connection.lastPong = DateTime.UtcNow;
            if (!connection.pings.TryDequeue(out var pingTime)) connection.Disconnect();
            connection.ping = (connection.lastPong - pingTime).TotalMilliseconds;
        }
    }
}
