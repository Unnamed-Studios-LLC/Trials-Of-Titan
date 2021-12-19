using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;
using Utils.NET.Net;

namespace TitanDatabase.Broadcasting.Packets
{
    public class BrGiveGold : BrPacket, ITokenPacket
    {
        public override BrPacketType Type => BrPacketType.GiveGold;

        public int Token { get; set; }

        public bool TokenResponse { get; set; }

        public ulong accountId;

        public int amount;

        public BrGiveGold()
        {

        }

        public BrGiveGold(ulong accountId, int amount)
        {
            this.accountId = accountId;
            this.amount = amount;
        }

        protected override void Read(BitReader r)
        {
            accountId = r.ReadUInt64();
            amount = r.ReadInt32();
        }

        protected override void Write(BitWriter w)
        {
            w.Write(accountId);
            w.Write(amount);
        }
    }
}
