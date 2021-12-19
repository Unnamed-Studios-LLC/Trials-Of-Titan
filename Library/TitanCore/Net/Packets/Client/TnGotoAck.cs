using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;

namespace TitanCore.Net.Packets.Client
{
    public class TnGotoAck : TnPacket, ITimePacket
    {
        public uint GetTime() => clientTickId * NetConstants.Client_Delta;

        public override TnPacketType Type => TnPacketType.GotoAck;

        public uint clientTickId;

        public TnGotoAck()
        {

        }

        public TnGotoAck(uint clientTickId)
        {
            this.clientTickId = clientTickId;
        }

        protected override void Read(BitReader r)
        {
            clientTickId = r.ReadUInt32();
        }

        protected override void Write(BitWriter w)
        {
            w.Write(clientTickId);
        }
    }
}
