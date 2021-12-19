using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Net;
using TitanCore.Net.Packets.Client;

namespace World.Net.Handling
{
    public class HitWallHandler : ClientPacketHandler<TnHitWall>
    {
        public override void Handle(TnHitWall packet, Client connection)
        {
            connection.player.gameState.PlayerHitWall(packet.clientTickId * NetConstants.Client_Delta, packet.projId, packet.x, packet.y);
        }
    }
}
