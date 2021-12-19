using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;

namespace TitanCore.Net.Packets.Server
{
    public class TnChangeMusic : TnPacket
    {
        public override TnPacketType Type => TnPacketType.ChangeMusic;

        public string musicName;

        public TnChangeMusic()
        {

        }

        public TnChangeMusic(string musicName)
        {
            this.musicName = musicName;
        }

        protected override void Read(BitReader r)
        {
            musicName = r.ReadUTF(40);
        }

        protected override void Write(BitWriter w)
        {
            w.Write(musicName);
        }
    }
}
