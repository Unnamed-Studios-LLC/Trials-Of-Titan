using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Net.Packets.Client;

namespace World.Net.Handling
{
    public class UseAbilityHandler : ClientPacketHandler<TnUseAbility>
    {
        public override void Handle(TnUseAbility packet, Client connection)
        {
            connection.player.gameState.UseAbility(packet);
        }
    }
}
