using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;

namespace TitanCore.Net.Packets.Server
{
    public class TnError : TnPacket
    {
        public override TnPacketType Type => TnPacketType.Error;

        public string message;

        public TnError()
        {

        }

        public TnError(string message)
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
