using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;

namespace TitanCore.Net.Packets.Client
{
    public class TnProjectilesAck : TnPacket, ITimePacket
    {
        public uint GetTime() => clientTickId * NetConstants.Client_Delta;

        public override TnPacketType Type => TnPacketType.ProjectilesAck;

        public uint clientTickId;

        public uint tickId;

        public TnProjectilesAck()
        {

        }

        public TnProjectilesAck(uint clientTickId, uint tickId)
        {
            this.clientTickId = clientTickId;
            this.tickId = tickId;
        }

        protected override void Read(BitReader r)
        {
            clientTickId = r.ReadUInt32();
            tickId = r.ReadUInt32();
        }

        protected override void Write(BitWriter w)
        {
            w.Write(clientTickId);
            w.Write(tickId);
        }
    }
}
