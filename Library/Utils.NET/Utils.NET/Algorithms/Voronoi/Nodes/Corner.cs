using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils.NET.Geometry;
using Utils.NET.Pathfinding;

namespace Utils.NET.Algorithms.Voronoi.Nodes
{
    public class Corner : IPathNode<Corner>
    {
        public int id;

        public Vec2 position;

        public HashSet<Center> touches = new HashSet<Center>();

        public HashSet<Edge> protrudes = new HashSet<Edge>();

        public HashSet<Corner> adjacent = new HashSet<Corner>();

        public Corner downSlope;

        public Corner river;

        public bool water = false;

        public bool coast = false;

        public float distanceFromBeach = -1;

        public float distanceFromLowest = -1;

        public float difficulty = 0;

        public float elevation = 0;

        public Corner(Vec2 position, int id)
        {
            this.position = position;
            this.id = id;
        }

        public Vec2 Position => position;

        public IEnumerable<Corner> Adjacent
        {
            get
            {
                foreach (var adj in adjacent)
                    if (!adj.coast)
                        yield return adj;
            }
        }
    }
}
