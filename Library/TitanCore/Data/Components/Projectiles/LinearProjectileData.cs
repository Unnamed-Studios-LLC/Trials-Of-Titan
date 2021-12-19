using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.Geometry;

namespace TitanCore.Data.Components.Projectiles
{
    public class LinearProjectileData : ProjectileData
    {
        public override ProjectileType Type => ProjectileType.Linear;

        public override Vec2 GetPosition(float time, float sin, float cos, uint projId)
        {
            return Vec2.FromAngle(sin, cos) * speed * time;
        }
    }
}
