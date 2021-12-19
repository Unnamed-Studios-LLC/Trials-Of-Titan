using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;

namespace TitanCore.Net.Packets.Server
{
    public abstract class TnIdPacket : TnPacket
    {
        /// <summary>
        /// The tick id of this packet
        /// </summary>
        public uint tickId;

        public TnIdPacket()
        {

        }

        public TnIdPacket(uint tickId)
        {
            this.tickId = tickId;
        }

        protected override void Read(BitReader r)
        {
            tickId = r.ReadUInt32();
        }

        protected override void Write(BitWriter w)
        {
            w.Write(tickId);
        }
    }
}
