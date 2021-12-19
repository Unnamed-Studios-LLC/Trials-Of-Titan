using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;

namespace TitanCore.Net.Packets.Client
{
    public class TnDrop : TnPacket
    {
        public override TnPacketType Type => TnPacketType.Drop;

        /// <summary>
        /// The id of the container
        /// </summary>
        public uint gameId;

        /// <summary>
        /// The slot to drop
        /// </summary>
        public byte slot;

        public TnDrop()
        {

        }

        public TnDrop(uint gameId, byte slot)
        {
            this.gameId = gameId;
            this.slot = slot;
        }

        protected override void Read(BitReader r)
        {
            gameId = r.ReadUInt32();
            slot = r.ReadUInt8();
        }

        protected override void Write(BitWriter w)
        {
            w.Write(gameId);
            w.Write(slot);
        }
    }
}
