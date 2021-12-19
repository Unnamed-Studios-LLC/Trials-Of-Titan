using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;

namespace TitanCore.Net.Packets.Client
{
    public class TnHitWall : TnPacket, ITimePacket
    {
        public uint GetTime() => clientTickId * NetConstants.Client_Delta;

        public override TnPacketType Type => TnPacketType.HitWall;

        public uint clientTickId;

        public uint projId;

        public ushort x;

        public ushort y;

        public TnHitWall()
        {

        }

        public TnHitWall(uint clientTickId, uint projId, ushort x, ushort y)
        {
            this.clientTickId = clientTickId;
            this.projId = projId;
            this.x = x;
            this.y = y;
        }

        protected override void Read(BitReader r)
        {
            clientTickId = r.ReadUInt32();
            projId = r.ReadUInt32();
            x = r.ReadUInt16();
            y = r.ReadUInt16();
        }

        protected override void Write(BitWriter w)
        {
            w.Write(clientTickId);
            w.Write(projId);
            w.Write(x);
            w.Write(y);
        }
    }
}
