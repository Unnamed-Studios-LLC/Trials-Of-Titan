using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Net.Packets.Models;
using Utils.NET.IO;

namespace TitanCore.Net.Packets.Server
{
    public class TnTradeUpdate : TnPacket
    {
        public override TnPacketType Type => TnPacketType.TradeUpdate;

        public int version;

        public bool accepted;

        public TradeOffer offer;

        public TnTradeUpdate()
        {

        }

        public TnTradeUpdate(int version, bool accepted, TradeOffer offer)
        {
            this.version = version;
            this.accepted = accepted;
            this.offer = offer;
        }

        protected override void Read(BitReader r)
        {
            version = r.ReadInt32();
            accepted = r.ReadBool();
            offer = TradeOffer.ReadOffer(r);
        }

        protected override void Write(BitWriter w)
        {
            w.Write(version);
            w.Write(accepted);
            offer.Write(w);
        }
    }
}
