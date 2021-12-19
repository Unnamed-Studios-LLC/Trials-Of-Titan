using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Net;
using TitanCore.Net.Packets.Client;

namespace World.Net.Handling
{
    public class EnemyHitWallHandler : ClientPacketHandler<TnEnemyHitWall>
    {
        public override void Handle(TnEnemyHitWall packet, Client connection)
        {
            if (connection.player.gameState == null) return;
            connection.player.gameState.EnemyHitWall(packet.clientTickId * NetConstants.Client_Delta, packet.projectileId);
        }
    }
}
