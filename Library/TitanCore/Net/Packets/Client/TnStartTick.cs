using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;

namespace TitanCore.Net.Packets.Client
{
    public class TnStartTick : TnPacket
    {
        public override TnPacketType Type => TnPacketType.StartTick;

        public uint clientTickId;

        public TnStartTick(uint clientTickId)
        {

        }

        public TnStartTick()
        {

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
