using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Net.Packets.Models;
using TitanCore.Net.Packets.Server;

namespace World.Net.Handling
{
    public class TradeResultHandler : ClientPacketHandler<TnTradeResult>
    {
        public override void Handle(TnTradeResult packet, Client connection)
        {
            if (packet.result == TradeResult.Cancelled)
                connection.player.CancelTrade();
        }
    }
}
