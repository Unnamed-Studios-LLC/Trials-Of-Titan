using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Net.Packets.Client;

namespace World.Net.Handling
{
    public class GotoAckHandler : ClientPacketHandler<TnGotoAck>
    {
        public override void Handle(TnGotoAck packet, Client connection)
        {
            connection.player?.GotoAck(packet.clientTickId);
        }
    }
}
