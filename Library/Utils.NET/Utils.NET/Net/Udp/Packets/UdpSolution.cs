using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;

namespace Utils.NET.Net.Udp.Packets
{
    public class UdpSolution : UdpPacket
    {
        public override UdpPacketType Type => UdpPacketType.Solution;

        /// <summary>
        /// The combination salt of the server and client generated salt
        /// </summary>
        public ulong salt;

        public UdpSolution() { }

        public UdpSolution(ulong salt) => this.salt = salt;

        protected override void Read(BitReader r)
        {
            salt = r.ReadUInt64();
        }

        protected override void Write(BitWriter w)
        {
            w.Write(salt);
        }
    }
}
