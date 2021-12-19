using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;
using Utils.NET.Net;

namespace ServerNode.Net.Packets
{
    public class NVersionCheck : NPacket, ITokenPacket
    {
        public override NPacketType Type => NPacketType.VersionCheck;

        public int Token { get; set; }

        public bool TokenResponse { get; set; }

        public int programType;

        public byte[] checksum;

        public NVersionCheck()
        {
        }

        public NVersionCheck(int programType, byte[] checksum)
        {
            this.programType = programType;
            this.checksum = checksum;
        }

        protected override void Read(BitReader r)
        {
            programType = r.ReadInt32();
            checksum = r.ReadBytes(r.ReadInt32());
        }

        protected override void Write(BitWriter w)
        {
            w.Write(programType);
            w.Write(checksum.Length);
            w.Write(checksum);
        }
    }
}
