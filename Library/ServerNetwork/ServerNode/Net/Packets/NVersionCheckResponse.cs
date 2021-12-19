using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;
using Utils.NET.Net;

namespace ServerNode.Net.Packets
{
    public class NVersionCheckResponse : NPacket, ITokenPacket
    {
        public override NPacketType Type => NPacketType.VersionCheckResponse;

        public int Token { get; set; }

        public bool TokenResponse { get; set; }

        public bool needsUpdate;

        public NVersionCheckResponse()
        {

        }

        public NVersionCheckResponse(bool needsUpdate)
        {
            this.needsUpdate = needsUpdate;
        }

        protected override void Read(BitReader r)
        {
            needsUpdate = r.ReadBool();
        }

        protected override void Write(BitWriter w)
        {
            w.Write(needsUpdate);
        }
    }
}
