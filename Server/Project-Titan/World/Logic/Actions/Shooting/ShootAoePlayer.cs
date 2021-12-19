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

    public class ShootAoePlayerValue
    {
        public object cooldownValue;
    }

    public class ShootAoePlayer : LogicAction<ShootAoePlayerValue>
    {

        /// <summary>
        /// The index of the projectile to shoot
        /// </summary>
        private byte index;

        /// <summary>
        /// The angle to offset from shooting at the player
        /// </summary>
        private float angleOffset;

        /// <summary>
        /// The angle gap between projectiles
        /// </summary>
        private float angleGap = 0;

        /// <summary>
        /// The amount of projectiles to shoot
        /// </summary>
        private int amount = 1;

        /// <summary>
        /// The radius range to shoot
        /// </summary>
        private Range radiusOffset;

        /// <summary>
        /// The cooldown of the shots
        /// </summary>
        private Cooldown cooldown = new Cooldown(0, -1);

        /// <summary>
        /// The radius to discover players
        /// </summary>
        private float searchRadius = 8;


        private ProjectileData data;

        public override bool ReadParameterValue(string name, LogicScriptReader reader)
        {
            switch (name)
            {
                case "index":
                    index = (byte)reader.ReadInt();
                    return true;
                case "angleOffset":
                    angleOffset = reader.ReadAngle();
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
                case "radiusOffset":
                    radiusOffset.min = radiusOffset.max = reader.ReadFloat();
                    return true;
                case "radiusOffsetMin":
                    radiusOffset.min = reader.ReadFloat();
                    return true;
                case "radiusOffsetMax":
                    radiusOffset.max = reader.ReadFloat();
                    return true;
            }
            if (cooldown.ReadParameterValue(name, reader))
                return true;
            return false;
        }

        public override void Init(Entity entity, out ShootAoePlayerValue obj, ref StateContext context, ref WorldTime time)
        {
            obj = new ShootAoePlayerValue();
            if (!(entity is Enemy enemy)) return;

            if (data == null)
            {
                var enemyInfo = (EnemyInfo)enemy.info;
                data = enemyInfo.projectiles[index];
            }

            cooldown.Init(out obj.cooldownValue);
        }

        public override void Tick(Entity entity, ref ShootAoePlayerValue obj, ref StateContext context, ref WorldTime time)
        {
            if (!(entity is Enemy enemy)) return;
            if (cooldown.Tick(ref obj.cooldownValue, ref time))
            {
                var player = enemy.GetClosestPlayer(searchRadius);
                if (player == null) return;

                var vector = player.position.Value - enemy.position.Value;
                float radiusValue = vector.Length;
                radiusValue += radiusOffset.GetRandom();
                float angle = vector.Angle; // TODO some sort of prediction to better hit players
                foreach (var shootAngle in NetConstants.GetProjectileAngles(angle + angleOffset, angleGap, amount))
                    enemy.ShootAoe(Shoot.GetDamage(enemy.soulGroup, data), index, enemy.position.Value + Vec2.FromAngle(shootAngle) * radiusValue);
            }
        }
    }
}
