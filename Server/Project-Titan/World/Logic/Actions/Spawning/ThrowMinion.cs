using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using TitanCore.Data;
using TitanCore.Net;
using Utils.NET.Geometry;
using Utils.NET.Logging;
using World.Logic.Components;
using World.Logic.Reader;
using World.Logic.States;
using World.Map.Objects.Entities;

namespace World.Logic.Actions.Spawning
{
    public class ThrowMinionValue
    {
        public object cooldownValue;
    }

    public class ThrowMinion : LogicAction<ThrowMinionValue>
    {
        /// <summary>
        /// The type of minion to spawn
        /// </summary>
        private GameObjectInfo spawnInfo;

        /// <summary>
        /// The angle the minion is thrown at
        /// </summary>
        public float angle;

        /// <summary>
        /// The angle gap betweens throws
        /// </summary>
        public float angleGap;

        /// <summary>
        /// The amount of minions thrown in one throw
        /// </summary>
        public int amount = 1;

        /// <summary>
        /// The distance the minion is thrown
        /// </summary>
        public float distance;

        /// <summary>
        /// The duration that the minion is in the air
        /// </summary>
        public float duration = 1.5f;

        /// <summary>
        /// The cooldown between spawns
        /// </summary>
        private Cooldown cooldown = new Cooldown();

        public override bool ReadParameterValue(string name, LogicScriptReader reader)
        {
            switch (name)
            {
                case "name":
                    var typeName = reader.ReadString();
                    spawnInfo = GameData.GetObjectByName(typeName);

                    if (spawnInfo == null)
                        Log.Write("No object named: " + typeName);

                    return true;
                case "angle":
                    angle = reader.ReadAngle();
                    return true;
                case "angleGap":
                    angleGap = reader.ReadAngle();
                    return true;
                case "amount":
                    amount = reader.ReadInt();
                    return true;
                case "distance":
                    distance = reader.ReadFloat();
                    return true;
                case "duration":
                    duration = reader.ReadFloat();
                    return true;
            }
            if (cooldown.ReadParameterValue(name, reader))
                return true;
            return false;
        }

        public override void Init(Entity entity, out ThrowMinionValue obj, ref StateContext context, ref WorldTime time)
        {
            obj = new ThrowMinionValue();
            cooldown.Init(out obj.cooldownValue);
        }

        public override void Tick(Entity entity, ref ThrowMinionValue obj, ref StateContext context, ref WorldTime time)
        {
            if (!(entity is Enemy enemy) || spawnInfo == null) return;

            if (cooldown.Tick(ref obj.cooldownValue, ref time))
            {
                foreach (var shootAngle in NetConstants.GetProjectileAngles(angle, angleGap, amount))
                {
                    var target = entity.position.Value + Vec2.FromAngle(shootAngle) * distance;
                    entity.PlayEffect(new BombBlastWorldEffect(entity.gameId, target, 0.5f, duration));

                    var minion = enemy.world.objects.CreateEnemy(spawnInfo);
                    minion.position.Value = target;
                    enemy.world.objects.SpawnObject(minion, duration);
                    enemy.AddMinion(minion);
                }
            }
        }
    }
}
