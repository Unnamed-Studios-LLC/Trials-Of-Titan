using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Net;
using TitanCore.Net.Packets.Client;
using TitanCore.Net.Packets.Server;

namespace World.Net.Handling
{
    public class MoveHandler : ClientPacketHandler<TnMove>
    {
        public override void Handle(TnMove packet, Client connection)
        {
            if (connection.player == null || !connection.player.startTick) return;
            uint time = NetConstants.Client_Delta * packet.clientTickId;
            if (!connection.player.CanMoveTo(packet.position, time))
            {
                connection.SendAsync(new TnError("Walk failure"));
                return;
            }
            connection.player.MoveTo(time, packet.position);
            connection.player.gameState.playerState?.ability.OnMove(packet.position, time);
            connection.player.gameState.Acknowledge(time, packet.tickId);
        }
    }
}
