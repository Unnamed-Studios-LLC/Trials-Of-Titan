using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Net;
using TitanCore.Net.Packets.Client;

namespace World.Net.Handling
{
    public class ProjectilesAckHandler : ClientPacketHandler<TnProjectilesAck>
    {
        public override void Handle(TnProjectilesAck packet, Client connection)
        {
            connection.player.gameState.Acknowledge(packet.clientTickId * NetConstants.Client_Delta, packet.tickId);
        }
    }
}
