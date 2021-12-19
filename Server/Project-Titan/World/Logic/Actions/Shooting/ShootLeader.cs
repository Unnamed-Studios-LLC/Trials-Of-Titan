using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Data.Components.Projectiles;
using TitanCore.Data.Entities;
using TitanCore.Net;
using Utils.NET.Collections;
using Utils.NET.Geometry;
using World.Logic.Components;
using World.Logic.Reader;
using World.Logic.States;
using World.Map.Objects.Entities;

namespace World.Logic.Actions.Shooting
{

    public class ShootLeaderValue
    {
        public object cooldownValue;
    }

    public class ShootLeader : LogicAction<ShootLeaderValue>
    {
        private static int randIndex = 0;

        /// <summary>
        /// The index of the projectile to shoot
        /// </summary>
        private byte index;

        /// <summary>
        /// The angle to offset from shooting at the player
        /// </summary>
        private Range angleOffset;

        /// <summary>
        /// The angle gap between projectiles
        /// </summary>
        private float angleGap = 0;

        /// <summary>
        /// The amount of projectiles to shoot
        /// </summary>
        private int amount = 1;

        /// <summary>
        /// The cooldown of the shots
        /// </summary>
        private Cooldown cooldown = new Cooldown(0, 0);


        private ProjectileData data;

        public override bool ReadParameterValue(string name, LogicScriptReader reader)
        {
            switch (name)
            {
                case "index":
                    index = (byte)reader.ReadInt();
                    return true;
                case "angleOffset":
                    angleOffset.min = angleOffset.max = reader.ReadAngle();
                    return true;
                case "angleOffsetMin":
                    angleOffset.min = reader.ReadAngle();
                    return true;
                case "angleOffsetMax":
                    angleOffset.max = reader.ReadAngle();
                    return true;
                case "angleGap":
                    angleGap = reader.ReadAngle();
                    return true;
                case "amount":
                    amount = reader.ReadInt();
                    if (angleGap == 0 && amount > 1)
                        angleGap = AngleUtils.PI_2 / amount;
                    return true;
            }
            if (cooldown.ReadParameterValue(name, reader))
                return true;
            return false;
        }

        public override void Init(Entity entity, out ShootLeaderValue obj, ref StateContext context, ref WorldTime time)
        {
            obj = new ShootLeaderValue();
            if (!(entity is Enemy enemy)) return;

            if (data == null)
            {
                var enemyInfo = (EnemyInfo)enemy.info;
                data = enemyInfo.projectiles[index];
            }

            cooldown.Init(out obj.cooldownValue);
        }

        public override void Tick(Entity entity, ref ShootLeaderValue obj, ref StateContext context, ref WorldTime time)
        {
            if (!(entity is Enemy enemy)) return;
            if (cooldown.Tick(ref obj.cooldownValue, ref time))
            {
                if (enemy.leader == null) return;

                float angle = enemy.position.Value.AngleTo(enemy.leader.position.Value);

                foreach (var shootAngle in NetConstants.GetProjectileAngles(angle + angleOffset.GetRandom(), angleGap, amount))
                    enemy.Shoot(Shoot.GetDamage(enemy.soulGroup, data), index, shootAngle, enemy.position.Value);
            }
        }
    }
}
