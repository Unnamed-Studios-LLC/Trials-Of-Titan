using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Utils.NET.Utils;

namespace Utils.NET.Geometry
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Vec2
    {
        [FieldOffset(0)]
        public float x;

        [FieldOffset(4)]
        public float y;

        public float Length => (float)Math.Sqrt(x * x + y * y);
        public float SqrLength => x * x + y * y;
        public float Angle => (float)Math.Atan2(y, x);

        public Vec2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }


        public Vec2 Clamp(float min, float max) => Clamp(new Vec2(min, min), new Vec2(max, max));
        public Int2 Clamp(Vec2 low, Vec2 high) => new Vec2(x < low.x ? low.x : (x > high.x ? high.x : x), y < low.y ? low.y : (y > high.y ? high.y : y));

        public Vec2 Add(Vec2 vec) => new Vec2(x + vec.x, y + vec.y);
        public Vec2 Subtract(Vec2 vec) => new Vec2(x - vec.x, y - vec.y);
        public Vec2 Multiply(Vec2 vec) => new Vec2(x * vec.x, y * vec.y);
        public Vec2 Divide(Vec2 vec) => new Vec2(x / vec.x, y / vec.y);

        public Vec2 Add(float value) => new Vec2(x + value, y + value);
        public Vec2 Subtract(float value) => new Vec2(x - value, y - value);
        public Vec2 Multiply(float value) => new Vec2(x * value, y * value);
        public Vec2 Divide(float value) => new Vec2(x / value, y / value);

        public Vec2 RotateAround(Vec2 pivot, float radians) => Subtract(pivot).RotateOrigin(radians).Add(pivot);
        public Vec2 RotateAround(Vec2 pivot, float sin, float cos) => Subtract(pivot).RotateOrigin(sin, cos).Add(pivot);
        public Vec2 RotateOrigin(float radians) => RotateOrigin((float)Math.Sin(radians), (float)Math.Cos(radians));
        public Vec2 RotateOrigin(float sin, float cos) => new Vec2(x * cos - y * sin, x * sin + y * cos);

        public float AngleTo(Vec2 vec) => (float)Math.Atan2(vec.y - y, vec.x - x);
        public float DistanceTo(Vec2 vec) => vec.Subtract(this).Length;
        public float SqrDistanceTo(Vec2 vec) => vec.Subtract(this).SqrLength;

        public Vec2 Normalize()
        {
            var l = Length;
            return new Vec2(x / l, y / l);
        }

        public Vec2 Invert()
        {
            return this * -1;
        }

        public Vec2 ChangeLength(float length)
        {
            var currentLength = Length;
            return new Vec2((x / currentLength) * length, (y / currentLength) * length);
        }

        public Vec2 ChangeLength(float length, float currentLength)
        {
            return new Vec2((x / currentLength) * length, (y / currentLength) * length);
        }

        public bool LongerThan(float radius)
        {
            return SqrLength > radius * radius;
        }

        public bool RadiusContains(Vec2 position, float radius)
        {
            return !(position - this).LongerThan(radius);
        }

        public Int2 ToInt2() => new Int2((int)x, (int)y);

        public static Vec2 zero = new Vec2(0, 0);
        public static Vec2 one = new Vec2(1, 1);

        public static Vec2 randomAngle => FromAngle(Rand.AngleValue());

        public static Vec2 FromAngle(float radians) => new Vec2((float)Math.Cos(radians), (float)Math.Sin(radians));
        public static Vec2 FromAngle(float sin, float cos) => new Vec2(cos, sin);

        public static Vec2 operator +(Vec2 a, Vec2 b) => a.Add(b);
        public static Vec2 operator -(Vec2 a, Vec2 b) => a.Subtract(b);
        public static Vec2 operator *(Vec2 a, Vec2 b) => a.Multiply(b);
        public static Vec2 operator /(Vec2 a, Vec2 b) => a.Divide(b);

        public static Vec2 operator +(Vec2 a, float b) => a.Add(b);
        public static Vec2 operator -(Vec2 a, float b) => a.Subtract(b);
        public static Vec2 operator *(Vec2 a, float b) => a.Multiply(b);
        public static Vec2 operator /(Vec2 a, float b) => a.Divide(b);

        public static bool operator ==(Vec2 a, Vec2 b) => a.x == b.x && a.y == b.y;
        public static bool operator !=(Vec2 a, Vec2 b) => a.x != b.x || a.y != b.y;

        public static implicit operator Vec2(Int2 vec) => new Vec2(vec.x, vec.y);
        public static implicit operator Vec2(float value) => new Vec2(value, value);

        public override string ToString() => $"{{{x}, {y}}}";

        public override bool Equals(object obj)
        {
            if (obj is Vec2 o)
                return this == o;
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
