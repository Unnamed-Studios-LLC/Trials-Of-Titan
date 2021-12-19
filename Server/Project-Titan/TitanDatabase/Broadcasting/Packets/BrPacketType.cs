using System;
using System.Collections.Generic;
using System.Text;

namespace TitanDatabase.Broadcasting.Packets
{
    public enum BrPacketType
    {
        None,
        Purchase,
        PurchaseResponse,
        Message,
        Verify,
        ServerMessage,
        GiveGold,
        GiveGoldResponse
    }
}
