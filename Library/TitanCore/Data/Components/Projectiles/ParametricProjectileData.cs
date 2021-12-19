﻿using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.Geometry;
using Utils.NET.IO.Xml;

namespace TitanCore.Data.Components.Projectiles
{
    public class ParametricProjectileData : ProjectileData
    {
        public override ProjectileType Type => ProjectileType.Parametric;

        /// <summary>
        /// The radius of the parametric function
        /// </summary>
        public float radius;

        /// <summary>
        /// The amplitude of the wave generated by the parametric function
        /// </summary>
        public float amplitude;

        public override void Parse(XmlParser xml)
        {
            base.Parse(xml);

            radius = xml.Float("Radius", 1);
            amplitude = xml.Float("Amplitude", 1);
        }

        public override Vec2 GetPosition(float time, float sin, float cos, uint projId)
        {
            time = time / lifetime;
            var position = new Vec2((float)Math.Sin(time * AngleUtils.PI_2), (float)Math.Cos(4 * time * AngleUtils.PI + AngleUtils.PI / 2)) * radius;
            position.y *= amplitude;
            float offset = ((projId % 4) / 4.0f) * AngleUtils.PI_2;
            return position.RotateOrigin((float)Math.Asin(sin) + offset);
        }
    }
}
