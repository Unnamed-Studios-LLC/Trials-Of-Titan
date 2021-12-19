using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Net.Packets.Models;
using Utils.NET.Collections;

namespace WorldGen.Rasterization
{
    public class WorldDefinition
    {
        public BiomeDefinition ocean;

        public BiomeDefinition beach;

        public RangeMap<BiomeDefinition> biomes = new RangeMap<BiomeDefinition>();
    }

    public class BiomeDefinition
    {
        private RangeMap<ushort> tileTypes = new RangeMap<ushort>();

        private RangeMap<ushort> objectTypes = new RangeMap<ushort>();

        public float perlinScale;

        public BiomeDefinition(float perlinScale, RangePair<ushort>[] tiles, RangePair<ushort>[] objects)
        {
            this.perlinScale = perlinScale;
            foreach (var tile in tiles)
                tileTypes.Add(tile.range, tile.value);

            if (objects != null)
                foreach (var obj in objects)
                    objectTypes.Add(obj.range, obj.value);
        }

        public ushort GetTile(float value)
        {
            return tileTypes[value];
        }

        public ushort GetObject(float value)
        {
            return objectTypes[value];
        }
    }
}
