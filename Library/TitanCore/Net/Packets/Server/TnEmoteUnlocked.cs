using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using Utils.NET.IO;

namespace TitanCore.Net.Packets.Server
{
    public class TnEmoteUnlocked : TnPacket
    {
        public override TnPacketType Type => TnPacketType.EmoteUnlocked;

        public ushort emoteType;

        public TnEmoteUnlocked()
        {

        }

        public TnEmoteUnlocked(ushort emoteType)
        {
            this.emoteType = emoteType;
        }

        protected override void Read(BitReader r)
        {
            emoteType = r.ReadUInt16();
        }

        protected override void Write(BitWriter w)
        {
            w.Write(emoteType);
        }
    }
}
