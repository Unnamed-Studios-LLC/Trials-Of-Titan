using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.Geometry;
using Utils.NET.IO;

namespace TitanCore.Net.Packets.Client
{
    public class TnHit : TnPacket, ITimePacket
    {
        public uint GetTime() => clientTickId * NetConstants.Client_Delta;

        public override TnPacketType Type => TnPacketType.Hit;

        /// <summary>
        /// The tick id of the client
        /// </summary>
        public uint clientTickId;

        /// <summary>
        /// The id of the projectile that hit
        /// </summary>
        public uint projectileId;

        /// <summary>
        /// The id of the entity that was hit
        /// </summary>
        public uint entityId;

        /// <summary>
        /// The position of the player
        /// </summary>
        public Vec2 position;

        public TnHit()
        {

        }

        public TnHit(uint clientTickId, uint projectileId, uint entityId, Vec2 position)
        {
            this.clientTickId = clientTickId;
            this.projectileId = projectileId;
            this.entityId = entityId;
            this.position = position;
        }

        protected override void Read(BitReader r)
        {
            clientTickId = r.ReadUInt32();
            projectileId = r.ReadUInt32();
            entityId = r.ReadUInt32();
            position = r.ReadVec2();
        }

        protected override void Write(BitWriter w)
        {
            w.Write(clientTickId);
            w.Write(projectileId);
            w.Write(entityId);
            w.Write(position);
        }
    }
}
