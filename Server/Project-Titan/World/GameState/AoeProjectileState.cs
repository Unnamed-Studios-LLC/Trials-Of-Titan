using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Data;
using TitanCore.Data.Components.Projectiles;
using TitanCore.Data.Entities;
using TitanCore.Data.Items;
using TitanCore.Net;
using TitanCore.Net.Packets.Models;
using Utils.NET.Geometry;
using World.Map.Objects.Entities;
using World.Net;

namespace World.GameState
{
    public class AoeProjectileState
    {
        public AoeProjectileData data;

        public uint ownerId;

        public GameObjectInfo ownerInfo;

        public uint projectileId;

        public ushort damage;

        public Vec2 target;

        public bool didHitPlayer = false;

        public uint endTime = uint.MaxValue;

        public HashSet<uint> hitSet = new HashSet<uint>();

        public HashSet<uint> wallHitSet = new HashSet<uint>();

        public AoeProjectileState(Enemy enemy, EnemyAoeProjectile projectile)
        {
            ownerInfo = enemy.info;
            data = (AoeProjectileData)((EnemyInfo)enemy.info).projectiles[projectile.index];

            ownerId = projectile.ownerId;
            projectileId = projectile.projectileId;
            damage = projectile.damage;
            target = projectile.target;
        }

        public void SetEndTime(uint time)
        {
            endTime = NetConstants.GetAoeExpireTime(time, Client.Client_Fixed_Delta, data.lifetime);
        }

        public AoeProjectileState(uint time, AllyAoeProjectile projectile)
        {
            var item = (WeaponInfo)GameData.objects[projectile.item];
            data = (AoeProjectileData)item.projectiles[projectile.projectileId % item.projectiles.Length];

            ownerId = projectile.ownerId;
            projectileId = projectile.projectileId;
            damage = projectile.damage;
            target = projectile.target;

            endTime = NetConstants.GetAoeExpireTime(time, Client.Client_Fixed_Delta, data.lifetime);
        }
    }
}
