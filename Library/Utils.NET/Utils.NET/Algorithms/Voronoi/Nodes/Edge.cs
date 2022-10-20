using System;
using System.Collections.Generic;
using System.Text;

namespace Utils.NET.Algorithms.Voronoi.Nodes
{
    public class Edge
    {
        public int id;

        public Center d0;

        public Center d1;

        public Corner v0;

        public Corner v1;

        public Edge(int id)
        {
            this.id = id;
        }
    }
}
