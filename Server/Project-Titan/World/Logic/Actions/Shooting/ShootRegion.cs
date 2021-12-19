using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using TitanCore.Data.Components.Projectiles;
using TitanCore.Data.Entities;
using TitanCore.Net;
using Utils.NET.Collections;
using Utils.NET.Geometry;
using Utils.NET.Utils;
using World.Logic.Components;
using World.Logic.Reader;
using World.Logic.States;
using World.Map.Objects.Entities;

namespace World.Logic.Actions.Shooting
{
    public class ShootRegionValue
    {
        public object cooldownValue;
    }

    public class ShootRegion : LogicAction<ShootRegionValue>
    {
        /// <summary>
        /// The index of the projectile to shoot
        /// </summary>
        private byte index;

        /// <summary>
        /// The angle to shoot at
        /// </summary>
        private Range angle;

        /// <summary>
        /// The angle gap between projectiles
        /// </summary>
        private float angleGap = 0;

        /// <summary>
        /// The amount of projectiles to shoot
        /// </summary>
        private int amount = 1;

        /// <summary>
        /// Offset to shoot from
        /// </summary>
        private Vec2 offset;

        private Region region;

        /// <summary>
        /// The cooldown of the shots
        /// </summary>
        private Cooldown cooldown = new Cooldown();

        private ProjectileData data;

        public override bool ReadParameterValue(string name, LogicScriptReader reader)
        {
            switch (name)
            {
                case "index":
                    index = (byte)reader.ReadInt();
                    return true;
                case "angle":
                    angle.min = angle.max = reader.ReadAngle();
                    return true;
                case "angleMin":
                    angle.min = reader.ReadAngle();
                    return true;
                case "angleMax":
                    angle.max = reader.ReadAngle();
                    return true;
                case "angleGap":
                    angleGap = reader.ReadAngle();
                    return true;
                case "amount":
                    amount = reader.ReadInt();
                    if (angleGap == 0 && amount > 1)
                        angleGap = AngleUtils.PI_2 / amount;
                    return true;
                case "offsetX":
                    offset.x = reader.ReadFloat();
                    return true;
                case "offsetY":
                    offset.y = reader.ReadFloat();
                    return true;
                case "region":
                    Enum.TryParse(reader.ReadString(), true, out region);
                    return true;
            }
            if (cooldown.ReadParameterValue(name, reader))
                return true;
            return false;
        }

        public override void Init(Entity entity, out ShootRegionValue obj, ref StateContext context, ref WorldTime time)
        {
            obj = new ShootRegionValue();
            if (!(entity is Enemy enemy)) return;

            if (data == null)
            {
                var enemyInfo = (EnemyInfo)enemy.info;
                data = enemyInfo.projectiles[index];
            }

            cooldown.Init(out obj.cooldownValue);
        }

        public override void Tick(Entity entity, ref ShootRegionValue obj, ref StateContext context, ref WorldTime time)
        {
            if (!(entity is Enemy enemy)) return;
            if (cooldown.Tick(ref obj.cooldownValue, ref time))
            {
                var position = entity.world.GetClosestRegion(region, entity.position.Value);
                foreach (var shootAngle in NetConstants.GetProjectileAngles(angle.GetRandom(), angleGap, amount))
                    enemy.Shoot(GetDamage(enemy.soulGroup, data), index, shootAngle, position + offset);
            }
        }

        public static ushort GetDamage(SoulGroup group, ProjectileData data)
        {
            var groupDamage = SoulGroupDefinitions.GetDamageValue(group);
            var min = groupDamage * data.minDamageMod;
            var max = groupDamage * data.maxDamageMod;

            return (ushort)(min + (max - min) * Rand.FloatValue());
        }
    }
}
