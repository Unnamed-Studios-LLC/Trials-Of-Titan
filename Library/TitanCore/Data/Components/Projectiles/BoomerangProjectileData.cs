using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.Geometry;
using Utils.NET.IO.Xml;

namespace TitanCore.Data.Components.Projectiles
{
    public class BoomerangProjectileData : ProjectileData
    {
        public override ProjectileType Type => ProjectileType.Boomerang;

        /// <summary>
        /// The turn-around point for the projectile (0.0 <-> 1.0, 1.0 = Lifetime)
        /// </summary>
        public float reverse;

        public override void Parse(XmlParser xml)
        {
            base.Parse(xml);

            reverse = xml.Float("reverse", 0.5f);

            var range = xml.Float("Range", 0);
            if (range > 0)
            {
                lifetime = (range * 2) / speed;
            }
        }

        public override Vec2 GetPosition(float time, float sin, float cos, uint projId)
        {
            float turnTime = lifetime * reverse;
            if (time > turnTime)
            {
                Vec2 pos = Vec2.FromAngle(sin, cos) * speed;
                return (pos * turnTime) - (pos * (time - turnTime));
            }
            return Vec2.FromAngle(sin, cos) * speed * time;
        }
    }
}
