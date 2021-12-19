using BenTools.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TitanCore.Net.Packets.Models;
using Utils.NET;
using Utils.NET.Geometry;
using Utils.NET.Logging;
using Utils.NET.Partitioning;
using Utils.NET.Pathfinding;
using Utils.NET.Utils;
using WorldGen.Nodes;
using WorldGen.Rasterization;

namespace WorldGen
{
    public class World
    {
        private const float Perlin_Scale = 2f;

        /// <summary>
        /// The width of the world in tiles
        /// </summary>
        public int width;

        /// <summary>
        /// The height of the world in tiles
        /// </summary>
        public int height;

        /// <summary>
        /// The random object for this world
        /// </summary>
        public Random random;

        /// <summary>
        /// The random used to generate the map shape
        /// </summary>
        private Random shapeRandom;

        public List<Center> centers = new List<Center>();

        public List<Corner> corners = new List<Corner>();

        public List<Edge> edges = new List<Edge>();

        public List<Center> landCenters = new List<Center>();

        public List<Corner> landCorners = new List<Corner>();

        public HashSet<Corner> rivers = new HashSet<Corner>();

        public HashSet<List<Vec2>> roads = new HashSet<List<Vec2>>();

        public HashSet<Center> towns = new HashSet<Center>();

        private Center[] pixelMap;

        private int centerIds;

        private int cornerIds;

        private int edgeIds;

        private Vec2 elevationOffset;

        public World(int width, int height, int seed)
        {
            this.width = width;
            this.height = height;
            random = new Random(seed);
            shapeRandom = new Random(seed + 1);
        }

        public void Generate(int pointCount, int relaxations)
        {
            var offset = new Vec2((float)(random.NextDouble()) * 100000, (float)(random.NextDouble()) * 100000);
            elevationOffset = new Vec2((float)(random.NextDouble()) * 100000, (float)(random.NextDouble()) * 100000);

            var points = new Vector[pointCount];
            for (int i = 0; i < points.Length; i++)
                points[i] = new Vector(shapeRandom.NextDouble() * width, shapeRandom.NextDouble() * height);

            GenerateNodes(points);
            for (int i = 0; i < relaxations; i++)
            {
                GenerateNodes(RelaxPoints());
            }

            GeneratePixelMap();

            Assign(offset);
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

        public Center GetCenterNear(int x, int y)
        {
            if (x < 0) x = 0;
            if (x >= width) x = width - 1;
            if (y < 0) y = 0;
            if (y >= height) y = height - 1;

            return pixelMap[y * width + x];
        }

        private void Assign(Vec2 offset)
        {
            AssignLand(offset);
            AssignDistanceFromBeach();
            AssignElevation();
            AssignRivers(random.Next(5, 7));
            AssignDistanceFromLowest();
            AssignDifficulty();

            AssignCenterAverages();

            AssignTowns();
        }

        private void AssignLand(Vec2 offset)
        {
            foreach (var center in centers)
            {
                center.water = !IsLand(center, offset);
            }

            foreach (var center in centers)
            {
                center.coast = !center.water && center.neighbors.Any(_ => _.water);
            }

            RemoveIslands();

            foreach (var corner in corners)
            {
                corner.water = corner.touches.All(_ => _.water);
                corner.coast = !corner.water && corner.touches.Any(_ => _.water);
            }

            landCenters = centers.Where(_ => !_.water).ToList();
            landCorners = corners.Where(_ => !_.water).ToList();
        }

        private bool IsLand(Center center, Vec2 offset)
        {
            var p = (center.position - (width / 2)) / width * 2;
            var perlin = (float)Perlin.Noise(offset.x + p.x * Perlin_Scale, offset.y + p.y * Perlin_Scale, 0) - 0.5f;
            var n = -(p * new Vec2(1.3f, 1)).Length + 0.65f + perlin * 0.9f;
            return n > 0;
        }

        private void RemoveIslands()
        {
            HashSet<Center> mainland = new HashSet<Center>();
            var first = GetCenterNear(width / 2, height / 2);
            mainland.Add(first);

            Queue<Center> toAssign = new Queue<Center>();
            toAssign.Enqueue(first);

            while (toAssign.Count > 0)
            {
                var center = toAssign.Dequeue();
                foreach (var neighbor in center.neighbors)
                    if (!neighbor.water && mainland.Add(neighbor))
                        toAssign.Enqueue(neighbor);
            }

            foreach (var center in centers)
            {
                if (mainland.Contains(center)) continue;

                center.water = true;
                center.coast = false;
            }
        }

        private void AssignDistanceFromBeach()
        {
            Queue<Corner> toAssign = new Queue<Corner>();

            var firstCorner = corners.First(_ => _.water);
            firstCorner.distanceFromBeach = 0;
            toAssign.Enqueue(firstCorner);

            while (toAssign.Count > 0)
            {
                var corner = toAssign.Dequeue();
                var distance = corner.distanceFromBeach + 1;
                foreach (var neighbor in corner.adjacent)
                {
                    if (neighbor.water)
                    {
                        if (neighbor.distanceFromBeach == 0) continue;
                        neighbor.distanceFromBeach = 0;
                        toAssign.Enqueue(neighbor);
                        continue;
                    }

                    if (neighbor.distanceFromBeach <= 0)
                    {
                        neighbor.distanceFromBeach = distance;
                        toAssign.Enqueue(neighbor);
                        continue;
                    }

                    if (neighbor.distanceFromBeach > distance)
                    {
                        neighbor.distanceFromBeach = distance;
                        toAssign.Enqueue(neighbor);
                        continue;
                    }
                }
            }

            var max = corners.Max(_ => _.distanceFromBeach);
            foreach (var corner in corners)
                corner.distanceFromBeach /= max;
        }

        private void AssignElevation()
        {
            foreach (var corner in landCorners)
            {
                var p = (corner.position / width) * 9;
                var perlin = (float)Perlin.Noise(elevationOffset.x + p.x, elevationOffset.y + p.y, 0);
                corner.elevation = corner.distanceFromBeach * 1.2f + perlin;
            }

            var max = landCorners.Max(_ => _.elevation);
            foreach (var corner in landCorners)
                corner.elevation /= max;

            foreach (var corner in landCorners)
                corner.elevation *= corner.elevation;

            foreach (var corner in landCorners)
            {
                corner.downSlope = corner.adjacent.Closest(_ => _.elevation);
                if (corner.downSlope.elevation > corner.elevation)
                {
                    corner.downSlope = corner.adjacent.Closest(_ => _.distanceFromBeach);
                }
            }
        }

        private void AssignRivers(int count)
        {
            var starts = landCorners.Where(_ => _.elevation > 0.5f).ToArray();
            for (int i = 0; i < count; i++)
            {
                Corner corner = null;

                int bc = 0;
                do corner = starts[random.Next(starts.Length)];
                while ((rivers.Contains(corner) || corner.downSlope == null) && bc++ < 100);

                if (bc >= 100) return;

                rivers.Add(corner);

                while (corner.downSlope != null && !corner.coast)
                {
                    if (corner.river != null) break;
                    corner.river = corner.downSlope;
                    corner = corner.river;
                }
            }
        }

        private void AssignDistanceFromLowest()
        {
            Queue<Corner> toAssign = new Queue<Corner>();
            var lowest = landCorners.Min((a, b) => a.position.y < b.position.y ? a : b);
            lowest.distanceFromLowest = 0;
            toAssign.Enqueue(lowest);

            while (toAssign.Count > 0)
            {
                var corner = toAssign.Dequeue();
                var distance = corner.distanceFromLowest + 1;
                foreach (var neighbor in corner.adjacent)
                {
                    if (neighbor.water) continue;

                    if (neighbor.distanceFromLowest <= 0)
                    {
                        neighbor.distanceFromLowest = distance;
                        toAssign.Enqueue(neighbor);
                        continue;
                    }

                    if (neighbor.distanceFromLowest > distance)
                    {
                        neighbor.distanceFromLowest = distance;
                        toAssign.Enqueue(neighbor);
                        continue;
                    }
                }
            }

            var max = landCorners.Max(_ => _.distanceFromLowest);
            foreach (var corner in landCorners)
                corner.distanceFromLowest /= max;
        }

        private void AssignDifficulty()
        {
            float max = landCorners.Max(_ =>
            {
                _.difficulty = _.distanceFromBeach + _.distanceFromLowest * 2.5f;
                return _.difficulty;
            });
            foreach (var corner in landCorners)
                corner.difficulty /= max;
        }

        private void AssignCenterAverages()
        {
            foreach (var center in centers)
            {
                center.distanceFromBeach = center.corners.Average(_ => _.distanceFromBeach);
                center.distanceFromLowest = center.corners.Average(_ => _.distanceFromLowest);
                center.difficulty = center.corners.Average(_ => _.difficulty);
                center.elevation = center.corners.Average(_ => _.elevation);
            }
        }

        private void AssignTowns()
        {
            AssignTownAt(0);
            AssignTownAt(0.3f, _ => _.coast);
            AssignTownAt(0.6f);
            AssignTownAt(0.9f);

            AssignTownRegions();
            AssignRoads();
        }

        private void AssignTownAt(float difficulty, Func<Center, bool> constraint = null)
        {
            IEnumerable<Center> collection = landCenters;
            if (constraint != null)
                collection = landCenters.Where(constraint);
            var center = collection.Closest(_ => Math.Abs(_.difficulty - difficulty));
            center.town = true;
            towns.Add(center);
        }

        private void AssignTownRegions()
        {
            foreach (var center in landCenters)
            {
                center.townRegion = towns.Closest(_ => _.position.DistanceTo(center.position));
            }
        }

        private void AssignRoads()
        {
            var towns = this.towns.OrderBy(_ => _.difficulty).ToArray();
            for (int i = 0; i < towns.Length - 1; i++)
            {
                AssignRoad(towns[i], towns[i + 1]);
            }
        }

        private void AssignRoad(Center a, Center b)
        {
            var closestACorner = a.corners.Closest(_ => _.coast ? float.MaxValue : (_.position - b.position).SqrLength);
            var closestBCorner = b.corners.Closest(_ => _.coast ? float.MaxValue : (_.position - a.position).SqrLength);
            var path = AStar.Pathfind(closestACorner, closestBCorner);
            if (path == null)
            {
                Log.Write("Failed to pathfind road");
                return;
            }

            List<Vec2> road = new List<Vec2>();
            road.Add(a.position);
            road.AddRange(path.Select(_ => _.position));
            road.Add(b.position);

            roads.Add(road);
        }

        private void GeneratePixelMap()
        {
            pixelMap = new Center[width * height];
            var partitions = new ArrayPartitionMap<PartitionedCenter>(width, height, width / 20);
            foreach (var center in centers)
                partitions.Add(new PartitionedCenter(center));
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    long index = y * width + x;
                    pixelMap[index] = GetClosestCenter(partitions, x, y);
                }
            }
        }

        private Center GetClosestCenter(ArrayPartitionMap<PartitionedCenter> partitions, int x, int y)
        {
            Center closest = null;
            float distance = 9999999;

            foreach (var partitioned in partitions.GetNearObjects(new Rect(x, y, 0, 0)))
            {
                var dis = partitioned.center.position.DistanceTo(new Vec2(x, y));
                if (dis < distance)
                {
                    distance = dis;
                    closest = partitioned.center;
                }
            }

            return closest;
        }

        private class PartitionedCenter : IPartitionable
        {
            public Rect BoundingRect { get; private set; }

            public IntRect LastBoundingRect { get; set; }

            public Center center;

            public PartitionedCenter(Center center)
            {
                this.center = center;
                BoundingRect = Rect.FromBounds(center.corners.Select(_ => _.position));
            }
        }

        #region Rasterization

        public MapTile[,] Rasterize(WorldDefinition definition)
        {
            var offset = new Vec2(Rand.FloatValue(), Rand.FloatValue());

            var tiles = new MapTile[width, height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var center = GetCenterNear(x, y);
                    var biome = (center.water ? definition.ocean : (center.coast ? definition.beach : definition.biomes[center.difficulty])) ?? definition.beach;

                    float biomePerlin = (float)Perlin.Noise((offset.x + x / (float)width) * biome.perlinScale, (offset.y + y / (float)height) * biome.perlinScale, 0);
                    var tile = new MapTile((ushort)x, (ushort)y, biome.GetTile(biomePerlin), biome.GetObject(biomePerlin));
                    tiles[x, y] = tile;
                }
            }
            return tiles;
        }

        #endregion
    }
}
