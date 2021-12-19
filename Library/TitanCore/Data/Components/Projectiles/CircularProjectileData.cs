using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.Geometry;
using Utils.NET.IO.Xml;

namespace TitanCore.Data.Components.Projectiles
{
    public class CircularProjectileData : ProjectileData
    {
        public override ProjectileType Type => ProjectileType.Circular;

        private float radius;

        private float arc;

        private bool clockwise;

        public override void Parse(XmlParser xml)
        {
            base.Parse(xml);

            radius = xml.Float("Radius");
            arc = xml.Float("Arc") * AngleUtils.Deg2Rad;
            clockwise = xml.Exists("Clockwise");

            lifetime = (arc * radius) / speed;
        }

        public override Vec2 GetPosition(float time, float sin, float cos, uint projId)
        {
            var scalar = clockwise ? -1 : 1;
            return Vec2.FromAngle((clockwise ? AngleUtils.PI : 0) + time * (speed / radius) * scalar).Multiply(radius).Subtract(new Vec2(radius * scalar, 0)).RotateOrigin(sin, cos);
        }
    }
}
