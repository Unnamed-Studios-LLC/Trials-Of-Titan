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
    public class ShootPlayerValue
    {
        public object cooldownValue;
    }

    public class ShootPlayer : LogicAction<ShootPlayerValue>
    {
        private static float[] rand;

        private static int randIndex = 0;

        static ShootPlayer()
        {
            rand = new float[100];
            for (int i = 0; i < rand.Length; i++)
                rand[i] = Rand.FloatValue();
        }

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
        private Cooldown cooldown = new Cooldown(0, -1);

        /// <summary>
        /// The radius to discover players
        /// </summary>
        private float searchRadius = 0;


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
                case "searchRadius":
                    searchRadius = reader.ReadFloat();
                    return true;
            }
            if (cooldown.ReadParameterValue(name, reader))
                return true;
            return false;
        }

        public override void Init(Entity entity, out ShootPlayerValue obj, ref StateContext context, ref WorldTime time)
        {
            obj = new ShootPlayerValue();
            if (!(entity is Enemy enemy)) return;

            if (data == null)
            {
                var enemyInfo = (EnemyInfo)enemy.info;
                data = enemyInfo.projectiles[index];

                if (searchRadius == 0)
                {
                    searchRadius = Math.Max(data.lifetime * data.speed, 10);
                }
            }

            cooldown.Init(out obj.cooldownValue);
        }

        public override void Tick(Entity entity, ref ShootPlayerValue obj, ref StateContext context, ref WorldTime time)
        {
            if (!(entity is Enemy enemy)) return;
            if (cooldown.Tick(ref obj.cooldownValue, ref time))
            {
                var player = enemy.GetClosestPlayer(searchRadius);
                if (player == null) return;

                float angle = enemy.position.Value.AngleTo(player.position.Value);//player.PredictPosition(0.1f * rand[(randIndex++) % rand.Length])); // TODO some sort of prediction to better hit players

                foreach (var shootAngle in NetConstants.GetProjectileAngles(angle + angleOffset.GetRandom(), angleGap, amount))
                    enemy.Shoot(Shoot.GetDamage(enemy.soulGroup, data), index, shootAngle, enemy.position.Value);
            }
        }
    }
}
