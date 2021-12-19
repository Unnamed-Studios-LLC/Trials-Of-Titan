using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;

namespace TitanCore.Net.Packets.Server
{
    public class TnWorldChange : TnPacket
    {
        public override TnPacketType Type => TnPacketType.WorldChange;

        protected override void Read(BitReader r)
        {

        }

        protected override void Write(BitWriter w)
        {

        }
    }
}
