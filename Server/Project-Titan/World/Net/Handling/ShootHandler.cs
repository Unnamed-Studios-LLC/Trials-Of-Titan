using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Net.Packets.Client;

namespace World.Net.Handling
{
    public class ShootHandler : ClientPacketHandler<TnShoot>
    {
        public override void Handle(TnShoot packet, Client connection)
        {
            connection.player.ProcessShoot(packet);
        }
    }
}
