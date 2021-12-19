using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;

namespace TitanCore.Net.Packets.Client
{
    public class TnEscape : TnPacket
    {
        public override TnPacketType Type => TnPacketType.Escape;

        public TnEscape()
        {

        }

        protected override void Read(BitReader r)
        {

        }

        protected override void Write(BitWriter w)
        {

        }
    }
}
