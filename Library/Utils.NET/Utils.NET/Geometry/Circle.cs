using System;
using System.Collections.Generic;
using System.Text;

namespace Utils.NET.Geometry
{
    public struct Circle
    {
        public Vec2 position;

        public float radius;

        public Circle(Vec2 position, float radius)
        {
            this.position = position;
            this.radius = radius;
        }

        public bool Contains(Vec2 point)
        {
            return (point - position).SqrLength < radius * radius;
        }
    }
}
