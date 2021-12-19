using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Net.Packets.Models;
using Utils.NET.IO;

namespace TitanCore.Net.Packets.Server
{
    public class TnTradeResult : TnPacket
    {
        public override TnPacketType Type => TnPacketType.TradeResult;

        public TradeResult result;

        public TnTradeResult()
        {

        }

        public TnTradeResult(TradeResult result)
        {
            this.result = result;
        }

        protected override void Read(BitReader r)
        {
            result = (TradeResult)r.ReadUInt8();
        }

        protected override void Write(BitWriter w)
        {
            w.Write((byte)result);
        }
    }
}
