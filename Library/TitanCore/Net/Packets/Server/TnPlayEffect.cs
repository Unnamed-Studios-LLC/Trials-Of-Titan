using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using Utils.NET.IO;

namespace TitanCore.Net.Packets.Server
{
    public class TnPlayEffect : TnPacket
    {
        public override TnPacketType Type => TnPacketType.PlayEffect;

        public WorldEffect effect;

        public TnPlayEffect()
        {

        }

        public TnPlayEffect(WorldEffect effect)
        {
            this.effect = effect;
        }

        protected override void Read(BitReader r)
        {
            effect = WorldEffect.Read(r);
        }

        protected override void Write(BitWriter w)
        {
            effect.Write(w);
        }
    }
}
