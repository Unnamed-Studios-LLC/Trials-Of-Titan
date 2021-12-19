using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;

namespace ServerNode.Net.Packets
{
    public class NNodeVerify : NPacket
    {
        public override NPacketType Type => NPacketType.NodeVerify;

        private static ulong Verification_Token = 895845505454562386;

        private ulong token;

        public NNodeVerify()
        {

        }

        protected override void Read(BitReader r)
        {
            token = r.ReadUInt64();
        }

        protected override void Write(BitWriter w)
        {
            w.Write(Verification_Token);
        }

        public bool IsValid() => token == Verification_Token;
    }
}
