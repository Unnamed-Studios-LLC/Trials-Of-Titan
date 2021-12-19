using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;

namespace World.Instances.Packets
{
    public class InVerify : InPacket
    {
        public override InPacketType Type => InPacketType.Verify;

        private const ulong Verify_Token = 573965739585678437;

        public ulong token;

        public string instanceId;

        public InVerify()
        {

        }

        protected override void Read(BitReader r)
        {
            token = r.ReadUInt64();
            instanceId = r.ReadUTF(40);
        }

        protected override void Write(BitWriter w)
        {
            w.Write(Verify_Token);
            w.Write(instanceId);
        }

        public bool IsValid()
        {
            return token == Verify_Token;
        }
    }
}
