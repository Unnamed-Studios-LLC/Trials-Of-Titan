using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;

namespace ServerNode.Net.Packets
{
    public class NProgramStop : NPacket
    {
        public override NPacketType Type => NPacketType.ProgramStop;

        public NProgramStop()
        {

        }

        protected override void Read(BitReader r)
        {

        }

        protected override void Write(BitWriter w)
        {

        }
    }
}
