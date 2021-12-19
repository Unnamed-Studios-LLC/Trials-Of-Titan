using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Net.Packets.Client;

namespace World.Net.Handling
{
    public class AllyHitHandler : ClientPacketHandler<TnAllyHit>
    {
        public override void Handle(TnAllyHit packet, Client connection)
        {

        }
    }
}
