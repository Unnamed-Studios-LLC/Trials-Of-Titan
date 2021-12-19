using System;
using TitanCore.Data;
using TitanCore.Data.Map;
using Utils.NET.IO;

namespace TitanCore.Net.Packets.Models
{
    /// <summary>
    /// Respresentation of a single tile/object pair on the map
    /// </summary>
    public struct MapTile
    {
        public static MapTile ReadTile(BitReader r)
        {
            var tile = new MapTile();
            tile.Read(r);
            return tile;
        }

        public ushort x;

        public ushort y;

        /// <summary>
        /// The type of ground tile represented
        /// </summary>
        public ushort tileType;

        /// <summary>
        /// The type of object that resides on the tile
        /// </summary>
        public ushort objectType;

        public MapTile(ushort x, ushort y, ushort tileType, ushort objectType)
        {
            this.x = x;
            this.y = y;
            this.tileType = tileType;
            this.objectType = objectType;
        }

        public void Read(BitReader r)
        {
            x = r.ReadUInt16();
            y = r.ReadUInt16();
            tileType = r.ReadUInt16();
            objectType = r.ReadUInt16();
        }

        public void Write(BitWriter w)
        {
            w.Write(x);
            w.Write(y);
            w.Write(tileType);
            w.Write(objectType);
        }

        public TileInfo GetTileInfo()
        {
            if (tileType == 0) return null;
            return (TileInfo)GameData.objects[tileType];
        }

        public GameObjectInfo GetObjectInfo()
        {
            if (objectType == 0) return null;
            return GameData.objects[objectType];
        }
    }
}
