using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.Geometry;
using Utils.NET.IO;

namespace TitanCore.Net.Packets.Server
{
    public class TnGoto : TnPacket
    {
        public override TnPacketType Type => TnPacketType.Goto;

        public Vec2 position;

        public TnGoto()
        {

        }

        public TnGoto(Vec2 position)
        {
            this.position = position;
        }

        protected override void Read(BitReader r)
        {
            position = r.ReadVec2();
        }

        protected override void Write(BitWriter w)
        {
            w.Write(position);
        }
    }
}
