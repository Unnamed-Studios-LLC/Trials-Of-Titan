using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.Algorithms.Voronoi.Nodes;
using Utils.NET.Geometry;

namespace Utils.NET.Algorithms.Voronoi
{
    public class VoronoiSet
    {
        public List<Center> centers = new List<Center>();

        public List<Corner> corners = new List<Corner>();

        public List<Edge> edges = new List<Edge>();

        private int centerIds;

        private int cornerIds;

        private int edgeIds;

        public VoronoiSet(Vec2[] positions, int relaxations)
        {
            var points = new Vector[positions.Length];
            for (int i = 0; i < points.Length; i++)
                points[i] = new Vector(positions[i].x, positions[i].y);

            GenerateNodes(points);
            for (int i = 0; i < relaxations; i++)
            {
                GenerateNodes(RelaxPoints());
            }
        }

        private void GenerateNodes(Vector[] points)
        {
            var voronoi = Fortune.ComputeVoronoiGraph(points);
            GenerateRelations(points, voronoi);
        }

        private Vector[] RelaxPoints()
        {
            var points = new Vector[centers.Count];
            int index = 0;
            foreach (var center in centers)
            {
                Vec2 p = new Vec2(0, 0);
                foreach (var corner in center.corners)
                    p += corner.position;
                p /= center.corners.Count;
                points[index++] = new Vector(p.x, p.y);
            }
            return points;
        }

        private void GenerateRelations(Vector[] points, VoronoiGraph voronoi)
        {
            centers.Clear();
            corners.Clear();
            edges.Clear();

            var centerMap = new Dictionary<Vector, Center>();
            var cornerMap = new Dictionary<Vector, Corner>();
            var edgeMap = new Dictionary<VoronoiEdge, Edge>();

            foreach (var point in points)
            {
                centerMap.Add(point, new Center(new Vec2((float)point[0], (float)point[1]), centerIds++));
            }
            foreach (var point in voronoi.Vertizes)
            {
                cornerMap.Add(point, new Corner(new Vec2((float)point[0], (float)point[1]), cornerIds++));
            }

            foreach (var edge in voronoi.Edges)
            {
                if (edge.IsPartlyInfinite || edge.VVertexA == Fortune.VVUnkown || edge.VVertexB == Fortune.VVUnkown) continue;

                var newEdge = new Edge(edgeIds++);
                edgeMap.Add(edge, newEdge);

                newEdge.d0 = centerMap[edge.LeftData];
                newEdge.d0.borders.Add(newEdge);

                newEdge.d1 = centerMap[edge.RightData];
                newEdge.d1.borders.Add(newEdge);

                newEdge.d0.neighbors.Add(newEdge.d1);
                newEdge.d1.neighbors.Add(newEdge.d0);

                newEdge.v0 = cornerMap[edge.VVertexA];
                newEdge.v0.protrudes.Add(newEdge);
                newEdge.v0.touches.Add(newEdge.d0);
                newEdge.v0.touches.Add(newEdge.d1);

                newEdge.v1 = cornerMap[edge.VVertexB];
                newEdge.v1.protrudes.Add(newEdge);
                newEdge.v1.touches.Add(newEdge.d0);
                newEdge.v1.touches.Add(newEdge.d1);

                newEdge.v0.adjacent.Add(newEdge.v1);
                newEdge.v1.adjacent.Add(newEdge.v0);

                newEdge.d0.corners.Add(newEdge.v0);
                newEdge.d0.corners.Add(newEdge.v1);

                newEdge.d1.corners.Add(newEdge.v0);
                newEdge.d1.corners.Add(newEdge.v1);
            }

            centers.AddRange(centerMap.Values);
            edges.AddRange(edgeMap.Values);
            corners.AddRange(cornerMap.Values);
        }
    }
}
