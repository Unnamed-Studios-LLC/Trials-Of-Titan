using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;

namespace TitanCore.Net.Packets.Client
{
    public class TnSwap : TnPacket
    {
        public override TnPacketType Type => TnPacketType.Swap;

        /// <summary>
        /// The owner to swap from
        /// </summary>
        public uint ownerA;

        /// <summary>
        /// The slot to swap from
        /// </summary>
        public uint slotA;

        /// <summary>
        /// The owner to swap to
        /// </summary>
        public uint ownerB;

        /// <summary>
        /// The slot to swap to
        /// </summary>
        public uint slotB;

        public TnSwap()
        {

        }

        public TnSwap(uint ownerA, uint slotA, uint ownerB, uint slotB)
        {
            this.ownerA = ownerA;
            this.slotA = slotA;
            this.ownerB = ownerB;
            this.slotB = slotB;
        }

        protected override void Read(BitReader r)
        {
            ownerA = r.ReadUInt32();
            slotA = r.ReadUInt32();
            ownerB = r.ReadUInt32();
            slotB = r.ReadUInt32();
        }

        protected override void Write(BitWriter w)
        {
            w.Write(ownerA);
            w.Write(slotA);
            w.Write(ownerB);
            w.Write(slotB);
        }
    }
}
