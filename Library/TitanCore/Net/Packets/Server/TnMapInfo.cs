using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;

namespace TitanCore.Net.Packets.Server
{
    public class TnMapInfo : TnPacket
    {
        public override TnPacketType Type => TnPacketType.MapInfo;

        public uint playerGameId;

        public string music;

        public string worldName;

        public int width;

        public int height;

        public int seed;

        public bool allowsPlayerTeleport;

        public int maxPlayerCount;

        public TnMapInfo()
        {

        }

        public TnMapInfo(uint playerGameId, string music, string worldName, int width, int height, int seed, bool allowsPlayerTeleport, int maxPlayerCount)
        {
            this.playerGameId = playerGameId;
            this.music = music;
            this.worldName = worldName;
            this.width = width;
            this.height = height;
            this.seed = seed;
            this.allowsPlayerTeleport = allowsPlayerTeleport;
            this.maxPlayerCount = maxPlayerCount;
        }

        protected override void Read(BitReader r)
        {
            playerGameId = r.ReadUInt32();
            music = r.ReadUTF(40);
            worldName = r.ReadUTF(40);
            width = r.ReadInt32();
            height = r.ReadInt32();
            seed = r.ReadInt32();
            allowsPlayerTeleport = r.ReadBool();
            maxPlayerCount = r.ReadInt32();
        }

        protected override void Write(BitWriter w)
        {
            w.Write(playerGameId);
            w.Write(music);
            w.Write(worldName);
            w.Write(width);
            w.Write(height);
            w.Write(seed);
            w.Write(allowsPlayerTeleport);
            w.Write(maxPlayerCount);
        }
    }
}
