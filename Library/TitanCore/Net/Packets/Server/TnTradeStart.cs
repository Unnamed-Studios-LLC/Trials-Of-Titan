using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using Utils.NET.IO;

namespace TitanCore.Net.Packets.Server
{
    public class TnTradeStart : TnPacket
    {
        public override TnPacketType Type => TnPacketType.TradeStart;

        public uint otherGameId;

        public Item[] items;

        public TnTradeStart()
        {

        }

        public TnTradeStart(uint otherGameId, Item[] items)
        {
            this.otherGameId = otherGameId;
            this.items = items;
        }

        protected override void Read(BitReader r)
        {
            otherGameId = r.ReadUInt32();
            items = new Item[8];

            for (int i = 0; i < items.Length; i++)
                items[i] = Item.ReadItem(r);
        }

        protected override void Write(BitWriter w)
        {
            w.Write(otherGameId);
            for (int i = 0; i < items.Length; i++)
                items[i].Write(w);
        }
    }
}
