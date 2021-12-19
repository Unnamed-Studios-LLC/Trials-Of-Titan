using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Net.Packets.Server;

namespace World.Net.Handling
{
    public class TradeUpdateHandler : ClientPacketHandler<TnTradeUpdate>
    {
        public override void Handle(TnTradeUpdate packet, Client connection)
        {
            connection.player.UpdateTrade(packet);
        }
    }
}
