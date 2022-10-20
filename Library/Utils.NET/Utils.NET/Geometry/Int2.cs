using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Utils.NET.Pathfinding;

namespace Utils.NET.Geometry
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Int2 : IPathNode<Int2>
    {
        [FieldOffset(0)]
        public int x;

        [FieldOffset(4)]
        public int y;

        public float Length => (float)Math.Sqrt(x * x + y * y);
        public float SqrLength => x * x + y * y;

        public Vec2 Position => new Vec2(x, y);

        public IEnumerable<Int2> Adjacent => new Int2[] { new Vec2(x + 1, y), new Vec2(x - 1, y), new Vec2(x, y + 1), new Vec2(x, y - 1) };

        public Int2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public Int2 Clamp(Int2 low, Int2 high) => new Int2(x < low.x ? low.x : (x > high.x ? high.x : x), y < low.y ? low.y : (y > high.y ? high.y : y));

        public Int2 Add(Int2 vec) => new Int2(x + vec.x, y + vec.y);
        public Int2 Subtract(Int2 vec) => new Int2(x - vec.x, y - vec.y);
        public Int2 Multiply(Int2 vec) => new Int2(x * vec.x, y * vec.y);
        public Int2 Divide(Int2 vec) => new Int2(x / vec.x, y / vec.y);

        public Int2 Add(int value) => new Int2(x + value, y + value);
        public Int2 Subtract(int value) => new Int2(x - value, y - value);
        public Int2 Multiply(int value) => new Int2(x * value, y * value);
        public Int2 Divide(int value) => new Int2(x / value, y / value);

        public float AngleTo(Int2 vec) => (float)Math.Atan2(vec.y - y, vec.x - x);
        public float DistanceTo(Int2 vec) => vec.Subtract(this).Length;

        public Vec2 ToVec2() => new Vec2(x, y);

        public static Int2 operator +(Int2 a, Int2 b) => a.Add(b);
        public static Int2 operator -(Int2 a, Int2 b) => a.Subtract(b);
        public static Int2 operator *(Int2 a, Int2 b) => a.Multiply(b);
        public static Int2 operator /(Int2 a, Int2 b) => a.Divide(b);

        public static Int2 operator +(Int2 a, int b) => a.Add(b);
        public static Int2 operator -(Int2 a, int b) => a.Subtract(b);
        public static Int2 operator *(Int2 a, int b) => a.Multiply(b);
        public static Int2 operator /(Int2 a, int b) => a.Divide(b);

        public static bool operator ==(Int2 a, Int2 b) => a.x == b.x && a.y == b.y;
        public static bool operator !=(Int2 a, Int2 b) => !(a == b);

        public static implicit operator Int2(Vec2 vec) => new Int2((int)vec.x, (int)vec.y);
        public static implicit operator Int2(int value) => new Int2(value, value);

        public static Int2 zero = new Int2(0, 0);

        public override string ToString() => $"{{{x}, {y}}}";

        public override bool Equals(object obj)
        {
            if (obj is Int2 o)
                return this == o;
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
