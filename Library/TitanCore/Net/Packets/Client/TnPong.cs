using System;
using Utils.NET.IO;

namespace TitanCore.Net.Packets.Client
{
    public class TnPong : TnPacket, ITimePacket
    {
        public uint GetTime() => clientTickId * NetConstants.Client_Delta;

        public override TnPacketType Type => TnPacketType.Pong;

        /// <summary>
        /// The client's tick id
        /// </summary>
        public uint clientTickId;

        public TnPong()
        {

        }

        public TnPong(uint clientTickId)
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
