using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils.NET.Geometry;

namespace TitanCore.Gen
{
    public class CellularMap : GenMap
    {
        private Random random;

        public CellularMap(int width, int height, int seed, float groundPercentage, int smoothing, int emptyMassRemoval, int groundMassRemoval, int smoothingRange, int maxLandmass, int extrude, int wallThickness) : 
            base(width, height, groundPercentage, smoothing, emptyMassRemoval, groundMassRemoval, smoothingRange, maxLandmass, extrude, wallThickness)
        {
            random = new Random(seed);
        }

        protected override float ValueAt(Int2 point)
        {
            return (float)random.NextDouble();
        }
    }
}
