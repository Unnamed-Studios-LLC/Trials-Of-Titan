using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.Geometry;
using Utils.NET.Pathfinding;

namespace Utils.NET.Algorithms.Voronoi.Nodes
{
    public class Center : IPathNode<Center>
    {
        public int id;

        public Vec2 position;

        public HashSet<Center> neighbors = new HashSet<Center>();

        public HashSet<Edge> borders = new HashSet<Edge>();

        public HashSet<Corner> corners = new HashSet<Corner>();

        public bool water = false;

        public bool coast = false;

        public float distanceFromBeach = -1;

        public float distanceFromLowest = -1;

        public float difficulty = -1;

        public float elevation = 0;

        public bool town = false;

        public Center townRegion;

        public Center(Vec2 position, int id)
        {
            this.position = position;
            this.id = id;
        }

        public Vec2 Position => position;

        public IEnumerable<Center> Adjacent => neighbors;
    }
}
