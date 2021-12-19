using System;
using System.Collections.Generic;
using System.Text;
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
    public class ShootAoeValue
    {
        public object cooldownValue;
    }

    public class ShootAoe : LogicAction<ShootAoeValue>
    {
        /// <summary>
        /// The index of the projectile to shoot
        /// </summary>
        private byte index;

        /// <summary>
        /// The angle to shoot at
        /// </summary>
        private float angle;

        /// <summary>
        /// The angle gap between projectiles
        /// </summary>
        private float angleGap = 0;

        /// <summary>
        /// The radius range to shoot
        /// </summary>
        private Range radius;

        /// <summary>
        /// The amount of projectiles to shoot
        /// </summary>
        private int amount = 1;

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
                    angle = reader.ReadAngle();
                    return true;
                case "angleGap":
                    angleGap = reader.ReadAngle();
                    return true;
                case "amount":
                    amount = reader.ReadInt();
                    if (angleGap == 0 && amount > 1)
                        angleGap = AngleUtils.PI_2 / amount;
                    return true;
                case "radius":
                    radius.min = radius.max = reader.ReadFloat();
                    return true;
                case "radiusMin":
                    radius.min = reader.ReadFloat();
                    return true;
                case "radiusMax":
                    radius.max = reader.ReadFloat();
                    return true;
            }
            if (cooldown.ReadParameterValue(name, reader))
                return true;
            return false;
        }

        public override void Init(Entity entity, out ShootAoeValue obj, ref StateContext context, ref WorldTime time)
        {
            obj = new ShootAoeValue();
            if (!(entity is Enemy enemy)) return;

            if (data == null)
            {
                var enemyInfo = (EnemyInfo)enemy.info;
                data = enemyInfo.projectiles[index];
            }

            cooldown.Init(out obj.cooldownValue);
        }

        public override void Tick(Entity entity, ref ShootAoeValue obj, ref StateContext context, ref WorldTime time)
        {
            if (!(entity is Enemy enemy)) return;
            if (cooldown.Tick(ref obj.cooldownValue, ref time))
            {
                float radiusValue = radius.GetRandom();
                foreach (var shootAngle in NetConstants.GetProjectileAngles(angle, angleGap, amount))
                    enemy.ShootAoe(Shoot.GetDamage(enemy.soulGroup, data), index, enemy.position.Value + Vec2.FromAngle(shootAngle) * radiusValue);
            }
        }
    }
}
