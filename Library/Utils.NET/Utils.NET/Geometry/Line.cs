using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.Partitioning;

namespace Utils.NET.Geometry
{
    public struct Line : IPartitionable
    {
        public Vec2 start;

        public Vec2 end;

        public Line(Vec2 start, Vec2 end)
        {
            this.start = start;
            this.end = end;

            LastBoundingRect = default;
        }

        public Rect BoundingRect => Rect.FromBoundsParams(start, end);

        public IntRect LastBoundingRect { get; set; }

        public Vec2 Intersection(Line other)
        {
            float a1 = end.y - start.y;
            float b1 = start.x - end.x;
            float c1 = a1 * start.x + b1 * start.y;

            float a2 = other.end.y - other.start.y;
            float b2 = other.start.x - other.end.x;
            float c2 = a2 * other.start.x + b2 * other.start.y;

            float determinant = a1 * b2 - a2 * b1;

            float x = (b2 * c1 - b1 * c2) / determinant;
            float y = (a1 * c2 - a2 * c1) / determinant;
            return new Vec2(x, y);
        }

        private int Orientation(Vec2 p, Vec2 q, Vec2 r)
        {
            float val = (q.y - p.y) * (r.x - q.x) - (q.x - p.x) * (r.y - q.y);

            if (val == 0) return 0; // colinear 

            return (val > 0) ? 1 : 2; // clock or counterclock wise 
        }

        private bool OnSegment(Vec2 p, Vec2 q, Vec2 r)
        {
            if (q.x <= Math.Max(p.x, r.x) && q.x >= Math.Min(p.x, r.x) && q.y <= Math.Max(p.y, r.y) && q.y >= Math.Min(p.y, r.y))
                return true;

            return false;
        }

        public bool DoesIntersect(Line other)
        {
            Vec2 p1 = start, q1 = end;
            Vec2 p2 = other.start, q2 = other.end;

            // Find the four orientations needed for general and 
            // special cases 
            int o1 = Orientation(p1, q1, p2);
            int o2 = Orientation(p1, q1, q2);
            int o3 = Orientation(p2, q2, p1);
            int o4 = Orientation(p2, q2, q1);

            // General case 
            if (o1 != o2 && o3 != o4)
                return true;

            // Special Cases 
            // p1, q1 and p2 are colinear and p2 lies on segment p1q1 
            if (o1 == 0 && OnSegment(p1, p2, q1)) return true;

            // p1, q1 and q2 are colinear and q2 lies on segment p1q1 
            if (o2 == 0 && OnSegment(p1, q2, q1)) return true;

            // p2, q2 and p1 are colinear and p1 lies on segment p2q2 
            if (o3 == 0 && OnSegment(p2, p1, q2)) return true;

            // p2, q2 and q1 are colinear and q1 lies on segment p2q2 
            if (o4 == 0 && OnSegment(p2, q1, q2)) return true;

            return false; // Doesn't fall in any of the above cases 
        }
    }
}
