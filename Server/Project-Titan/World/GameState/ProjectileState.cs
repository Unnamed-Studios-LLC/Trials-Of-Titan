using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Data;
using TitanCore.Data.Components.Projectiles;
using TitanCore.Data.Entities;
using TitanCore.Data.Items;
using TitanCore.Net.Packets.Models;
using Utils.NET.Geometry;
using World.Map.Objects.Entities;

namespace World.GameState
{
    public class ProjectileState
    {
        public ProjectileData data;

        public uint ownerId;

        public GameObjectInfo ownerInfo;

        public uint projectileId;

        public ushort damage;

        private float sin;

        private float cos;

        public float angle;

        public Vec2 position;

        public float radius = 0.25f;

        public uint startTime = uint.MaxValue;

        public float reach = 0;

        public uint hitTime = uint.MaxValue;

        public ProjectileState(Enemy enemy, EnemyProjectile projectile)
        {
            ownerInfo = enemy.info;
            data = ((EnemyInfo)enemy.info).projectiles[projectile.index];

            ownerId = projectile.ownerId;
            projectileId = projectile.projectileId;
            damage = projectile.damage;
            angle = projectile.angle;
            sin = (float)Math.Sin(projectile.angle);
            cos = (float)Math.Cos(projectile.angle);
            position = projectile.position;

            Init();
        }

        public ProjectileState(uint time, AllyProjectile projectile, Vec2 position)
        {
            var item = (WeaponInfo)GameData.objects[projectile.item];
            data = item.projectiles[projectile.projectileId % item.projectiles.Length];

            ownerId = projectile.ownerId;
            projectileId = projectile.projectileId;
            damage = projectile.damage;
            angle = projectile.angle;
            sin = (float)Math.Sin(projectile.angle);
            cos = (float)Math.Cos(projectile.angle);
            this.position = position;
            if (projectile.reach && (data.Type == ProjectileType.Linear || data.Type == ProjectileType.Sin))
            {
                reach = 1f / data.speed;
            }

            startTime = time;

            Init();
        }

        public float GetLifetime()
        {
            return data.lifetime + reach;
        }

        private void Init()
        {
            radius = data.size * 0.25f;
        }

        public Vec2 GetPosition(uint time)
        {
            return position + data.GetPosition((time - startTime) / 1000f, sin, cos, projectileId);
        }
    }
}
