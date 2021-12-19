using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using Utils.NET.Geometry;
using Utils.NET.IO.Xml;

namespace TitanCore.Data.Components.Projectiles
{
    public class AoeProjectileData : ProjectileData
    {
        public override ProjectileType Type => ProjectileType.Aoe;

        /// <summary>
        /// The radius of the blast
        /// </summary>
        public float radius;

        /// <summary>
        /// The color of the bomb
        /// </summary>
        public GameColor color;

        /// <summary>
        /// The range of this projectile
        /// </summary>
        public float range;

        public override void Parse(XmlParser xml)
        {
            base.Parse(xml);

            radius = xml.Float("Radius");
            color = GameColor.Parse(xml.String("Color", xml.AtrString("color")));
            lifetime = xml.Float("Lifetime", 1);
            range = xml.Float("Range", 6);
        }

        public override Vec2 GetPosition(float time, float sin, float cos, uint projId)
        {
            return Vec2.FromAngle(sin, cos) * speed * time;
        }
    }
}
