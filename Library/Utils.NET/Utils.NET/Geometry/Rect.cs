using System;
using System.Collections.Generic;
using System.Text;

namespace Utils.NET.Geometry
{
    public struct Rect
    {
        public static Rect FromBoundsParams(params Vec2[] bounds) => FromBounds(bounds);

        public static Rect FromBounds(IEnumerable<Vec2> bounds)
        {
            Vec2 min = new Vec2();
            Vec2 max = new Vec2();

            bool first = true;
            foreach (var bound in bounds)
            {
                if (first)
                {
                    min = max = bound;
                    first = false;
                }
                else
                {
                    if (bound.x < min.x)
                        min.x = bound.x;
                    if (bound.x > max.x)
                        max.x = bound.x;
                    if (bound.y < min.y)
                        min.y = bound.y;
                    if (bound.y > max.y)
                        max.y = bound.y;
                }
            }

            return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
        }

        public float x;

        public float y;

        public float width;

        public float height;


        public Vec2 BottomLeft => new Vec2(x, y);
        public Vec2 TopRight => new Vec2(x + width, y + height);


        public Rect(float x, float y, float width, float height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public Rect(Vec2 position, Vec2 size)
        {
            x = position.x;
            y = position.y;
            width = size.x;
            height = size.y;
        }

        /*
        public IntRect GetBoundingRect()
        {
            return new IntRect((int)Math.Floor(x), (int)Math.Floor(y), (int)Math.Ceiling(x + width), (int)Math.Ceiling(y + height));
        }
        */

        public IntRect GetIntRect()
        {
            Int2 bl = BottomLeft;
            Int2 tr = TopRight;

            return new IntRect(bl.x, bl.y, tr.x - bl.x, tr.y - bl.y);
        }

        public override string ToString() => $"{{ {x}, {y}, {width}, {height} }}";
    }
}
