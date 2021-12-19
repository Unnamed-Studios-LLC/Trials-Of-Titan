using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;

namespace TitanCore.Net.Packets.Client
{
    public class TnAllyHit : TnPacket, ITimePacket
    {
        public uint GetTime() => clientTickId * NetConstants.Client_Delta;

        public override TnPacketType Type => TnPacketType.AllyHit;

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

        public TnAllyHit()
        {

        }

        public TnAllyHit(uint clientTickId, uint projectileId, uint entityId)
        {
            this.clientTickId = clientTickId;
            this.projectileId = projectileId;
            this.entityId = entityId;
        }

        protected override void Read(BitReader r)
        {
            clientTickId = r.ReadUInt32();
            projectileId = r.ReadUInt32();
            entityId = r.ReadUInt32();
        }

        protected override void Write(BitWriter w)
        {
            w.Write(clientTickId);
            w.Write(projectileId);
            w.Write(entityId);
        }
    }
}
