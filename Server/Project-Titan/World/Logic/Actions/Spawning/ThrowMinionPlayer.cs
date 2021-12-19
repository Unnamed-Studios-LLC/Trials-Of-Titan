using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using TitanCore.Data;
using Utils.NET.Geometry;
using Utils.NET.Logging;
using Utils.NET.Utils;
using World.Logic.Components;
using World.Logic.Reader;
using World.Logic.States;
using World.Map.Objects.Entities;

namespace World.Logic.Actions.Spawning
{
    public class ThrowMinionPlayerValue
    {
        public object cooldownValue;
    }

    public class ThrowMinionPlayer : LogicAction<ThrowMinionPlayerValue>
    {
        /// <summary>
        /// The type of minion to spawn
        /// </summary>
        private GameObjectInfo spawnInfo;

        /// <summary>
        /// The player targeting system
        /// </summary>
        public TargetingSystem targetingSystem = new TargetingSystem();

        /// <summary>
        /// The duration that the minion is in the air
        /// </summary>
        public float duration = 1.5f;

        /// <summary>
        /// The radius to discover players
        /// </summary>
        private float searchRadius = 10;

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
                        Log.Error("No object named: " + typeName);

                    return true;
                case "duration":
                    duration = reader.ReadFloat();
                    return true;
                case "searchRadius":
                    searchRadius = reader.ReadFloat();
                    return true;

            }
            if (cooldown.ReadParameterValue(name, reader))
                return true;
            if (targetingSystem.ReadParameterValue(name, reader))
                return true;
            return false;
        }

        public override void Init(Entity entity, out ThrowMinionPlayerValue obj, ref StateContext context, ref WorldTime time)
        {
            obj = new ThrowMinionPlayerValue();
            cooldown.Init(out obj.cooldownValue);
        }

        public override void Tick(Entity entity, ref ThrowMinionPlayerValue obj, ref StateContext context, ref WorldTime time)
        {
            if (!(entity is Enemy enemy) || spawnInfo == null) return;

            if (cooldown.Tick(ref obj.cooldownValue, ref time))
            {
                var player = targetingSystem.GetPlayer(entity, searchRadius);
                if (player == null) return;

                var target = targetingSystem.GetTargetPosition(entity, player);

                entity.PlayEffect(new BombBlastWorldEffect(entity.gameId, target, 0.5f, duration));

                var minion = enemy.world.objects.CreateEnemy(spawnInfo);
                minion.position.Value = target;
                enemy.world.objects.SpawnObject(minion, duration);
                enemy.AddMinion(minion);
            }
        }
    }
}
