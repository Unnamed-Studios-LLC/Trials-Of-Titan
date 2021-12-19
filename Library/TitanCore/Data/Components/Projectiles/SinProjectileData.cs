using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.Geometry;
using Utils.NET.IO.Xml;

namespace TitanCore.Data.Components.Projectiles
{
    public class SinProjectileData : ProjectileData
    {
        public override ProjectileType Type => ProjectileType.Sin;

        /// <summary>
        /// Amount of sin waves per 1 unit distance
        /// </summary>
        public float frequency;

        /// <summary>
        /// The height of the sin wave from 0
        /// </summary>
        public float amplitude;

        /// <summary>
        /// The PI offset in the sin function
        /// </summary>
        public float offset;

        public override void Parse(XmlParser xml)
        {
            base.Parse(xml);

            frequency = xml.Float("Frequency", 1);
            amplitude = xml.Float("Amplitude", 1);
            offset = xml.Float("Offset", 0);
        }

        public override Vec2 GetPosition(float time, float sin, float cos, uint projId)
        {
            return new Vec2(time * speed, amplitude * (float)Math.Sin(((((projId % 2) == 1) ? AngleUtils.PI : 0) + time * AngleUtils.PI * 2 * (frequency / lifetime)) + (AngleUtils.PI * offset))).RotateOrigin(sin, cos);
        }
    }
}
