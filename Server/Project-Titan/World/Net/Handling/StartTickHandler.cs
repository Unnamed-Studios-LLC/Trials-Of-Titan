using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Net;
using TitanCore.Net.Packets.Client;

namespace World.Net.Handling
{
    public class StartTickHandler : ClientPacketHandler<TnStartTick>
    {
        public override void Handle(TnStartTick packet, Client connection)
        {
            connection.StartTick(packet.clientTickId * NetConstants.Client_Delta);
        }
    }
}
