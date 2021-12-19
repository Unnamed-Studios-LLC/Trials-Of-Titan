using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;
using Utils.NET.Net;

namespace World.Instances.Packets
{
    public class InWorldKeyResult : InPacket, ITokenPacket
    {
        public override InPacketType Type => InPacketType.WorldKeyResult;

        public int Token { get; set; }

        public bool TokenResponse { get; set; }

        public ulong key;

        public InWorldKeyResult()
        {

        }

        public InWorldKeyResult(ulong key)
        {
            this.key = key;
        }

        protected override void Read(BitReader r)
        {
            key = r.ReadUInt64();
        }

        protected override void Write(BitWriter w)
        {
            w.Write(key);
        }
    }
}
