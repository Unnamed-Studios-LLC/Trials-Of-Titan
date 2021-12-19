using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.Geometry;
using Utils.NET.IO;

namespace TitanCore.Net.Packets.Client
{
    public class TnShoot : TnPacket, ITimePacket
    {
        public uint GetTime() => clientTickId * NetConstants.Client_Delta;

        public override TnPacketType Type => TnPacketType.Shoot;

        public uint clientTickId;

        //public uint projectileId;

        public Vec2 target;

        public Vec2 position;

        public TnShoot()
        {

        }

        public TnShoot(uint clientTickId, uint projectileId, Vec2 target, Vec2 position)
        {
            this.clientTickId = clientTickId;
            //this.projectileId = projectileId;
            this.target = target;
            this.position = position;
        }

        protected override void Read(BitReader r)
        {
            clientTickId = r.ReadUInt32();
            //projectileId = r.ReadUInt32();
            target = r.ReadVec2();
            position = r.ReadVec2();
        }

        protected override void Write(BitWriter w)
        {
            w.Write(clientTickId);
            //w.Write(projectileId);
            w.Write(target);
            w.Write(position);
        }
    }
}
