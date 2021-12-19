using System;
using TitanCore.Net.Packets.Models;
using Utils.NET.IO;

namespace TitanCore.Net.Packets.Server
{
    public class TnTiles : TnPacket
    {
        public override TnPacketType Type => TnPacketType.Tiles;

        public MapTile[] tiles;

        public TnTiles()
        {
        }

        public TnTiles(MapTile[] tiles)
        {
            this.tiles = tiles;
        }

        protected override void Read(BitReader r)
        {
            tiles = new MapTile[r.ReadUInt16()];
            for (int i = 0; i < tiles.Length; i++)
                tiles[i] = MapTile.ReadTile(r);
        }

        protected override void Write(BitWriter w)
        {
            w.Write((ushort)tiles.Length);
            for (int i = 0; i < tiles.Length; i++)
                tiles[i].Write(w);
        }
    }
}
