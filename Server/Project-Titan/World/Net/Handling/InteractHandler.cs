using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Net;
using TitanCore.Net.Packets.Client;
using TitanCore.Net.Packets.Server;
using World.Map.Objects.Interfaces;

namespace World.Net.Handling
{
    public class InteractHandler : ClientPacketHandler<TnInteract>
    {
        public override void Handle(TnInteract packet, Client connection)
        {
            if (!connection.player.world.objects.TryGetObject(packet.objectGameId, out var obj)) return;
            if (connection.player.gameState.playerState != null && !connection.player.gameState.playerState.AdvancePosition(packet.position, packet.clientTickId * NetConstants.Client_Delta))
            {
                return;
            }
            if (!(obj is IInteractable interactable)) return;
            interactable.Interact(connection.player, packet);
        }
    }
}
