using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils.NET.Algorithms;
using Utils.NET.Geometry;
using Utils.NET.Partitioning;
using Utils.NET.Pathfinding;
using Utils.NET.Utils;

namespace World.GameState
{
    public class Sight
    {
        public const int Player_Sight_Radius = 18;

        private const int Ray_Count = 32;

        private static Int2[][] rays = new Int2[Ray_Count + 4][];

        private static Int2[] normSight;

        static Sight()
        {
            GenerateRayGroups();
            GenerateNormalSight();
        }

        private static void GenerateRayGroups()
        {
            for (int i = 0; i < Ray_Count; i++)
            {
                float start = ((float)i / Ray_Count) * AngleUtils.PI * 2;
                float end = ((float)(i + 1) / Ray_Count) * AngleUtils.PI * 2;

                var triangle = new Triangle(new Vec2(0, 0), Vec2.FromAngle(start) * 16, Vec2.FromAngle(end) * 16);
                var points = triangle.Rasterize();
                rays[i] = SortRay(points).ToArray();
            }

            var leftRay = new Int2[15];
            for (int i = 0; i < leftRay.Length; i++)
            {
                leftRay[i] = new Int2(i - 1, 0);
            }
            rays[Ray_Count] = leftRay;

            var rightRay = new Int2[15];
            for (int i = 0; i < rightRay.Length; i++)
            {
                rightRay[i] = new Int2(i + 1, 0);
            }
            rays[Ray_Count + 1] = rightRay;

            var upRay = new Int2[15];
            for (int i = 0; i < upRay.Length; i++)
            {
                upRay[i] = new Int2(0, i + 1);
            }
            rays[Ray_Count + 2] = upRay;

            var downRay = new Int2[15];
            for (int i = 0; i < downRay.Length; i++)
            {
                downRay[i] = new Int2(0, i - 1);
            }
            rays[Ray_Count + 3] = downRay;
        }

        private static void GenerateNormalSight()
        {
            var points = new List<Int2>();
            for (int y = -Player_Sight_Radius; y <= Player_Sight_Radius; y++)
            {
                for (int x = -Player_Sight_Radius; x <= Player_Sight_Radius; x++)
                {
                    var point = new Int2(x, y);
                    if (point.Length > Player_Sight_Radius) continue;
                    points.Add(point);
                }
            }
            normSight = points.ToArray();
        }

        private static IEnumerable<Int2> SortRay(IEnumerable<Int2> ray)
        {
            var hash = new HashSet<Int2>();
            var list = new List<Int2>();
            var rayPoints = ray.OrderBy(_ => _.SqrLength).ToArray();
            for (int i = 0; i < rayPoints.Length - 1; i++)
            {
                var start = rayPoints[i];
                var end = rayPoints[i + 1];
                foreach (var point in AStar.Pathfind(start, end))
                {
                    if (hash.Add(point))
                        list.Add(point);
                }
            }
            return list;
        }

        public Sight()
        {

        }

        public IEnumerable<Int2> GetSightPoints(float viewRadius, Int2 start, World world)
        {
            if (!world.LimitSight)
            {
                foreach (var point in normSight)
                    yield return point;
                yield break;
            }

            for (int i = 0; i < Ray_Count; i++)
            {
                bool collided = false;
                foreach (var point in rays[i])
                {
                    var p = start + point;
                    if (p.x < 0 || p.y < 0 || p.x >= world.width || p.y >= world.height) continue;
                    if (world.tiles.GetCollisionType(p.x, p.y).HasFlag(Map.CollisionType.SightWall))//world.objects.collision[p.x, p.y])
                    {
                        collided = true;
                        yield return point;
                    }
                    else if (collided) break;
                    else yield return point;
                }
            }
        }
    }
}
