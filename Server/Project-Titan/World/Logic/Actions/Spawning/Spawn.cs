using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Data;
using Utils.NET.Collections;
using Utils.NET.Geometry;
using Utils.NET.Logging;
using Utils.NET.Utils;
using World.Logic.Components;
using World.Logic.Reader;
using World.Logic.States;
using World.Map.Objects.Entities;

namespace World.Logic.Actions.Spawning
{
    public class SpawnValue
    {
        public object cooldownValue;
    }

    public class Spawn : LogicAction<SpawnValue>
    {
        /// <summary>
        /// The type of minion to spawn
        /// </summary>
        private GameObjectInfo spawnInfo;

        /// <summary>
        /// The amount of minions spawned per cooldown
        /// </summary>
        private int rate;

        /// <summary>
        /// The maximum amount of minions to be spawned
        /// </summary>
        private int max;

        /// <summary>
        /// The delay to spawn this minion
        /// </summary>
        private float spawnDelay = 0;

        /// <summary>
        /// The cooldown between spawns
        /// </summary>
        private Cooldown cooldown = new Cooldown();

        /// <summary>
        /// The position selection object
        /// </summary>
        private PositionSelection selection = new PositionCircleSelection(new Range(0, 1), new Range(0, AngleUtils.PI_2), new Range(0, 0));

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
                case "rate":
                    rate = reader.ReadInt();
                    return true;
                case "max":
                    max = reader.ReadInt();
                    return true;
                case "spawnDelay":
                    spawnDelay = reader.ReadFloat();
                    return true;
                case "selection":
                    if (Enum.TryParse(reader.ReadString(), true, out PositionSelectionType type))
                        selection = PositionSelection.Create(type);
                    return true;
            }
            if (cooldown.ReadParameterValue(name, reader))
                return true;
            if (selection.ReadParameterValue(name, reader))
                return true;
            return false;
        }

        public override void Init(Entity entity, out SpawnValue obj, ref StateContext context, ref WorldTime time)
        {
            obj = new SpawnValue();
            cooldown.Init(out obj.cooldownValue);
        }

        public override void Tick(Entity entity, ref SpawnValue obj, ref StateContext context, ref WorldTime time)
        {
            if (!(entity is Enemy enemy)) return;
            if (cooldown.Tick(ref obj.cooldownValue, ref time))
            {
                var count = enemy.CountMinionsOfType(spawnInfo.id);
                for (int i = count; (i < count + rate) && (i < max); i++)
                {
                    var minion = enemy.world.objects.CreateEnemy(spawnInfo);
                    minion.position.Value = enemy.position.Value + selection.GetRelativeSpawnPosition(enemy);
                    if (spawnDelay <= 0)
                        enemy.world.objects.SpawnObject(minion);
                    else
                        enemy.world.objects.SpawnObject(minion, spawnDelay);
                    enemy.AddMinion(minion);
                }
            }
        }
    }
}
