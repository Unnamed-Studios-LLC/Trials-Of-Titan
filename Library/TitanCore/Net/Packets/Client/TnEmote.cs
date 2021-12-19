using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using Utils.NET.IO;

namespace TitanCore.Net.Packets.Client
{
    public class TnEmote : TnPacket
    {
        public override TnPacketType Type => TnPacketType.Emote;

        public EmoteType emoteType;

        public TnEmote()
        {

        }

        public TnEmote(EmoteType emoteType)
        {
            this.emoteType = emoteType;
        }

        protected override void Read(BitReader r)
        {
            emoteType = (EmoteType)r.ReadUInt8();
        }

        protected override void Write(BitWriter w)
        {
            w.Write((byte)emoteType);
        }
    }
}
