using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;

namespace TitanCore.Net.Packets.Server
{
    public class TnCreateResponse : TnPacket
    {
        public override TnPacketType Type => TnPacketType.CreateResponse;

        public ulong characterId;

        public TnCreateResponse(ulong characterId)
        {
            this.characterId = characterId;
        }

        public TnCreateResponse()
        {

        }

        protected override void Read(BitReader r)
        {
            characterId = r.ReadUInt64();
        }

        protected override void Write(BitWriter w)
        {
            w.Write(characterId);
        }
    }
}
