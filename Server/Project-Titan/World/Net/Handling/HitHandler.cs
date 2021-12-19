using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Net;
using TitanCore.Net.Packets.Client;

namespace World.Net.Handling
{
    public class HitHandler : ClientPacketHandler<TnHit>
    {
        public override void Handle(TnHit packet, Client connection)
        {
            if (packet.entityId == connection.player.gameId)
                connection.player.gameState.EnemyHitPlayer(packet.clientTickId * NetConstants.Client_Delta, packet.projectileId, packet.clientTickId, packet.position);
            else
                connection.player.gameState.PlayerHitEnemy(packet.clientTickId * NetConstants.Client_Delta, packet.projectileId, packet.entityId);
        }
    }
}
