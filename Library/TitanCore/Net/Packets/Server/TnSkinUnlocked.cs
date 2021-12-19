using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;

namespace TitanCore.Net.Packets.Server
{
    public class TnSkinUnlocked : TnPacket
    {
        public override TnPacketType Type => TnPacketType.SkinUnlocked;

        public ushort skinType;

        public TnSkinUnlocked(ushort skinType)
        {
            this.skinType = skinType;
        }

        public TnSkinUnlocked()
        {

        }

        protected override void Read(BitReader r)
        {
            skinType = r.ReadUInt16();
        }

        protected override void Write(BitWriter w)
        {
            w.Write(skinType);
        }
    }
}
