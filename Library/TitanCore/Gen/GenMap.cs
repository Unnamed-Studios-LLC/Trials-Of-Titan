using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils.NET.Collections;
using Utils.NET.Geometry;

namespace TitanCore.Gen
{
    public abstract class GenMap : Map
    {
        public Int2[] smoothingDirections;

        private float groundPercentage;

        private int smoothing;

        private int emptyMassRemoval;

        private int groundMassRemoval;

        private int maxLandmass;

        private int extrude;

        private int wallThickness;

        public GenMap(int width, int height, float groundPercentage, int smoothing, int emptyMassRemoval, int groundMassRemoval, int smoothingRange, int maxLandmass, int extrude, int wallThickness) : base(width, height)
        {
            this.groundPercentage = groundPercentage;
            this.smoothing = smoothing;
            this.emptyMassRemoval = emptyMassRemoval;
            this.groundMassRemoval = groundMassRemoval;
            this.maxLandmass = maxLandmass;
            this.extrude = extrude;
            this.wallThickness = wallThickness;

            CreateSmoothingDirections(smoothingRange);
        }

        public void Generate()
        {
            InitData(groundPercentage);

            for (int i = 0; i < smoothing; i++)
            {
                Smooth(smoothingDirections);
            }

            foreach (var mass in GetMasses(MapElementType.Empty))
            {
                if (mass.Count >= emptyMassRemoval) continue;
                foreach (var point in mass)
                    data[point.x, point.y] = MapElementType.Ground;
            }

            foreach (var mass in GetMasses(MapElementType.Ground))
            {
                if (mass.Count >= groundMassRemoval) continue;
                foreach (var point in mass)
                    data[point.x, point.y] = MapElementType.Empty;
            }

            var groundMasses = GetLandmasses(maxLandmass);
            for (int i = 0; i < extrude; i++)
            {
                Extrude(MapElementType.Ground, groundMasses);
                groundMasses = GetLandmasses(maxLandmass);
            }

            for (int i = 0; i < wallThickness; i++)
            {
                Extrude(MapElementType.Wall, groundMasses);
                groundMasses = GetLandmasses(maxLandmass);
            }
        }

        private void CreateSmoothingDirections(int range)
        {
            int size = range * 2 + 1;
            smoothingDirections = new Int2[size * size - 1];
            int index = 0;
            for (int y = -range; y <= range; y++)
                for (int x = -range; x <= range; x++)
                    if (x != 0 || y != 0)
                        smoothingDirections[index++] = new Int2(x, y);
        }

        private void InitData(float groundPercentage)
        {
            foreach (var point in EachPoint())
                    data[point.x, point.y] = (ValueAt(point) <= groundPercentage) ? MapElementType.Ground : MapElementType.Empty;
        }

        protected abstract float ValueAt(Int2 point);
    }
}