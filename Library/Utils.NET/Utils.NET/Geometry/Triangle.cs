using System;
using System.Collections.Generic;
using System.Text;

namespace Utils.NET.Geometry
{
    public class Triangle : IPolygon
    {
        public Vec2[] points;

        public Triangle(Vec2[] points)
        {
            if (points == null)
                throw new ArgumentNullException("The given points are null!");
            if (points.Length != 3)
                throw new ArgumentException("The given points are not of length 3");
            this.points = points;
        }

        public Triangle(Vec2 a, Vec2 b, Vec2 c)
        {
            points = new Vec2[]
            {
                a,
                b,
                c
            };
        }

        public Rect BoundingRect => Rect.FromBoundsParams(points);

        public IntRect LastBoundingRect { get; set; }

        public bool Contains(Vec2 point)
        {
            float x1 = points[0].x, y1 = points[0].y;
            float x2 = points[1].x, y2 = points[1].y;
            float x3 = points[2].x, y3 = points[2].y;

            float a = ((y2 - y3) * (point.x - x3) + (x3 - x2) * (point.y - y3)) / ((y2 - y3) * (x1 - x3) + (x3 - x2) * (y1 - y3));
            float b = ((y3 - y1) * (point.x - x3) + (x1 - x3) * (point.y - y3)) / ((y2 - y3) * (x1 - x3) + (x3 - x2) * (y1 - y3));
            float c = 1 - a - b;

            //if (a == 0 || b == 0 || c == 0) Console.WriteLine("Point is on the side of the triangle");
            return a >= 0 && a <= 1 && b >= 0 && b <= 1 && c >= 0 && c <= 1;
        }

        public IEnumerable<Int2> Rasterize()
        {
            return PointsInTriangle(points[0], points[1], points[2]);
        }

        public Vec2[] GetPoints()
        {
            return points;
        }

        public Line[] GetEdges()
        {
            return new Line[]
            {
                new Line(points[0], points[1]),
                new Line(points[1], points[2]),
                new Line(points[2], points[0])
            };
        }

        private IEnumerable<Int2> PointsInTriangle(Int2 pt1, Int2 pt2, Int2 pt3)
        {
            if (pt1.y == pt2.y && pt1.y == pt3.y)
            {
                throw new ArgumentException("The given points must form a triangle.");
            }

            Vec2 tmp;

            if (pt2.x < pt1.x)
            {
                tmp = pt1;
                pt1 = pt2;
                pt2 = tmp;
            }

            if (pt3.x < pt2.x)
            {
                tmp = pt2;
                pt2 = pt3;
                pt3 = tmp;

                if (pt2.x < pt1.x)
                {
                    tmp = pt1;
                    pt1 = pt2;
                    pt2 = tmp;
                }
            }

            var baseFunc = CreateFunc(pt1, pt3);
            var line1Func = pt1.x == pt2.x ? (x => pt2.y) : CreateFunc(pt1, pt2);

            for (var x = pt1.x; x < pt2.x; x++)
            {
                int maxY;
                int minY = GetRange(line1Func(x), baseFunc(x), out maxY);

                for (var y = minY; y <= maxY; y++)
                {
                    yield return new Vec2(x, y);
                }
            }

            var line2Func = pt2.x == pt3.x ? (x => pt2.y) : CreateFunc(pt2, pt3);

            for (var x = pt2.x; x <= pt3.x; x++)
            {
                int maxY;
                int minY = GetRange(line2Func(x), baseFunc(x), out maxY);

                for (var y = minY; y <= maxY; y++)
                {
                    yield return new Vec2(x, y);
                }
            }
        }

        private int GetRange(float y1, float y2, out int maxY)
        {
            if (y1 < y2)
            {
                maxY = (int)Math.Floor(y2);
                return (int)Math.Ceiling(y1);
            }

            maxY = (int)Math.Floor(y1);
            return (int)Math.Ceiling(y2);
        }

        private Func<int, float> CreateFunc(Vec2 pt1, Vec2 pt2)
        {
            var y0 = pt1.y;

            if (y0 == pt2.y)
            {
                return x => y0;
            }

            var m = (float)(pt2.y - y0) / (pt2.x - pt1.x);

            return x => m * (x - pt1.x) + y0;
        }
    }
}
