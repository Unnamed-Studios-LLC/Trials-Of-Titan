using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;
using Utils.NET.Net;

namespace TitanDatabase.Broadcasting.Packets
{
    public class BrGiveGoldResponse : BrPacket, ITokenPacket
    {
        public override BrPacketType Type => BrPacketType.GiveGoldResponse;

        public int Token { get; set; }

        public bool TokenResponse { get; set; }

        public bool success;

        public BrGiveGoldResponse()
        {

        }

        public BrGiveGoldResponse(bool success)
        {
            this.success = success;
        }

        protected override void Read(BitReader r)
        {
            success = r.ReadBool();
        }

        protected override void Write(BitWriter w)
        {
            w.Write(success);
        }
    }
}
