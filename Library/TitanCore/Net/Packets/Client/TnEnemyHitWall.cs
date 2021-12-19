using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;

namespace TitanCore.Net.Packets.Client
{
    public class TnEnemyHitWall : TnPacket, ITimePacket
    {
        public override TnPacketType Type => TnPacketType.EnemyHitWall;

        public uint clientTickId;

        public uint projectileId;

        public TnEnemyHitWall()
        {

        }

        public TnEnemyHitWall(uint clientTickId, uint projectileId)
        {
            this.clientTickId = clientTickId;
            this.projectileId = projectileId;
        }

        protected override void Read(BitReader r)
        {
            clientTickId = r.ReadUInt32();
            projectileId = r.ReadUInt32();
        }

        protected override void Write(BitWriter w)
        {
            w.Write(clientTickId);
            w.Write(projectileId);
        }

        public uint GetTime()
        {
            return clientTickId * NetConstants.Client_Delta;
        }
    }
}
