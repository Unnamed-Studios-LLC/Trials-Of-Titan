using System;
using Utils.NET.IO;
using Utils.NET.Net;
using Utils.NET.Net.Udp;
using Utils.NET.Net.Udp.Packets;

namespace TitanCore.Net.Packets.Server
{
    public class TnFragment : TnPacket
    {
        /// <summary>
        /// The max size of each fragment packet
        /// </summary>
        public const int Fragment_Packet_Size = 400;

        public override TnPacketType Type => TnPacketType.Fragment;

        /// <summary>
        /// True if this packet is the last fragment
        /// </summary>
        public bool last;

        /// <summary>
        /// The data of the fragmented packet
        /// </summary>
        public byte[] data;

        public TnFragment()
        {
        }

        protected override void Read(BitReader r)
        {
            last = r.ReadBool();

            int length = r.ReadUInt16();
            if (length > Fragment_Packet_Size)
                throw new LengthCheckFailedException("Fragment length received was larger than the max fragment length");
            data = r.ReadBytes(length);
        }

        protected override void Write(BitWriter w)
        {
            w.Write(last);
            w.Write((ushort)data.Length);
            w.Write(data);
        }
    }
}
