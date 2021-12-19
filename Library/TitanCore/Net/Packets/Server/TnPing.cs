using System;
using Utils.NET.IO;

namespace TitanCore.Net.Packets.Server
{
    public class TnPing : TnPacket
    {
        public override TnPacketType Type => TnPacketType.Ping;

        public TnPing()
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
