using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;

namespace TitanCore.Net.Packets.Server
{
    public class TnTradeRequest : TnPacket
    {
        public override TnPacketType Type => TnPacketType.TradeRequest;

        public string fromPlayer;

        public TnTradeRequest()
        {

        }

        public TnTradeRequest(string fromPlayer)
        {
            this.fromPlayer = fromPlayer;
        }

        protected override void Read(BitReader r)
        {
            fromPlayer = r.ReadUTF(256);
        }

        protected override void Write(BitWriter w)
        {
            w.Write(fromPlayer);
        }
    }
}
