using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.Geometry;
using Utils.NET.IO;

namespace TitanCore.Net.Packets.Client
{
    public class TnMove : TnPacket, ITimePacket
    {
        public uint GetTime() => clientTickId * NetConstants.Client_Delta;

        public override TnPacketType Type => TnPacketType.Move;

        /// <summary>
        /// The client's tick id
        /// </summary>
        public uint clientTickId;

        /// <summary>
        /// The tick Id that this packet is responding to
        /// </summary>
        public uint tickId;

        /// <summary>
        /// The position to move to
        /// </summary>
        public Vec2 position;

        public TnMove()
        {

        }

        public TnMove(uint clientTickId, uint tickId, Vec2 position)
        {
            this.clientTickId = clientTickId;
            this.tickId = tickId;
            this.position = position;
        }

        protected override void Read(BitReader r)
        {
            clientTickId = r.ReadUInt32();
            tickId = r.ReadUInt32();
            position = r.ReadVec2();
        }

        protected override void Write(BitWriter w)
        {
            w.Write(clientTickId);
            w.Write(tickId);
            w.Write(position);
        }
    }
}
