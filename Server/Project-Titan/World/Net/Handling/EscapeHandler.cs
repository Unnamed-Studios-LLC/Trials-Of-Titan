using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Net.Packets.Client;

namespace World.Net.Handling
{
    public class EscapeHandler : ClientPacketHandler<TnEscape>
    {
        public override void Handle(TnEscape packet, Client connection)
        {
            connection.Nexus();
        }
    }
}
