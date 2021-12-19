using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;
using Utils.NET.Net;

namespace ServerNode.Net.Packets
{
    public class NUpdate : NPacket, ITokenPacket
    {
        public override NPacketType Type => NPacketType.Update;

        public int Token { get; set; }

        public bool TokenResponse { get; set; }

        public int programType;

        public byte[] checksum;

        public byte[] zip;

        public NUpdate()
        {

        }

        public NUpdate(int programType, byte[] checksum, byte[] zip)
        {
            this.programType = programType;
            this.checksum = checksum;
            this.zip = zip;
        }

        protected override void Read(BitReader r)
        {
            programType = r.ReadInt32();
            checksum = r.ReadBytes(r.ReadInt32());
            zip = r.ReadBytes(r.ReadInt32());
        }

        protected override void Write(BitWriter w)
        {
            w.Write(programType);
            w.Write(checksum.Length);
            w.Write(checksum);
            w.Write(zip.Length);
            w.Write(zip);
        }
    }
}
