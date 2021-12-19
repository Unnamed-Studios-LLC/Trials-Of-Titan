using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.Geometry;
using Utils.NET.Utils;

namespace TitanCore.Gen
{
    public class PerlinMap : GenMap
    {
        private float offset;

        private float scale;

        public PerlinMap(int width, int height, float offset, float scale, float groundPercentage, int smoothing, int emptyMassRemoval, int groundMassRemoval, int smoothingRange, int maxLandmass, int extrude, int wallThickness) : base(width, height, groundPercentage, smoothing, emptyMassRemoval, groundMassRemoval, smoothingRange, maxLandmass, extrude, wallThickness)
        {
        }

        protected override float ValueAt(Int2 point)
        {
            return (float)Perlin.Noise(offset + (point.x / (float)width) * scale, offset + (point.y / height) * scale, 0);
        }
    }
}
