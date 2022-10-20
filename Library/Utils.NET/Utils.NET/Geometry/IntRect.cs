using System;
using System.Collections.Generic;
using System.Text;

namespace Utils.NET.Geometry
{
    public struct IntRect
    {
        public int x;

        public int y;

        public int width;

        public int height;


        public Int2 BottomLeft => new Int2(x, y);
        public Int2 TopRight => new Int2(x + width, y + height);


        public IntRect(int x, int y, int width, int height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public IntRect(Int2 position, Int2 size)
        {
            x = position.x;
            y = position.y;
            width = size.x;
            height = size.y;
        }

        public static bool operator ==(IntRect a, IntRect b) => a.x == b.x && a.y == b.y && a.width == b.width && a.height == b.height;
        public static bool operator !=(IntRect a, IntRect b) => !(a == b);

        public override string ToString() => $"{{{x}, {y}, {width}, {height}}}";
    }
}
