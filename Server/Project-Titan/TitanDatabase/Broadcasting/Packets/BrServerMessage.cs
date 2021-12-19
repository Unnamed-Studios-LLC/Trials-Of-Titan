using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;

namespace TitanDatabase.Broadcasting.Packets
{
    public class BrServerMessage : BrPacket
    {
        public override BrPacketType Type => BrPacketType.ServerMessage;

        public string message;

        public BrServerMessage()
        {

        }

        public BrServerMessage(string message)
        {
            this.message = message;
        }

        protected override void Read(BitReader r)
        {
            message = r.ReadUTF(200);
        }

        protected override void Write(BitWriter w)
        {
            w.Write(message);
        }
    }
}
