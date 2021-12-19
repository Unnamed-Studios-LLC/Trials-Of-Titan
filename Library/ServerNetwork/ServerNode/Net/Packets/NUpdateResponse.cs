using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;
using Utils.NET.Net;

namespace ServerNode.Net.Packets
{
    public class NUpdateResponse : NPacket, ITokenPacket
    {
        public override NPacketType Type => NPacketType.UpdateResponse;

        public int Token { get; set; }

        public bool TokenResponse { get; set; }

        public bool success;

        public NUpdateResponse()
        {

        }

        public NUpdateResponse(bool success)
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
