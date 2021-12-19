using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Data;
using Utils.NET.Geometry;
using Utils.NET.Logging;
using World.Logic.Reader;
using World.Map.Objects.Entities;

namespace World.Logic.Actions.Death.Spawning
{
    public class DeathSpawn : DeathAction
    {
        private GameObjectInfo spawnInfo;

        private int amount = 1;

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
                case "amount":
                    amount = reader.ReadInt();
                    return true;
            }
            return false;
        }

        public override void OnDeath(Enemy enemy, Player killer, List<Damager> damagers)
        {
            for (int i = 0; i < amount; i++)
            {
                Spawn(enemy.world, GetSpawnPosition(enemy.position.Value, i));
            }
        }

        private Vec2 GetSpawnPosition(Vec2 enemyPosition, int index)
        {
            return enemyPosition;
        }

        private void Spawn(World world, Vec2 position)
        {
            var enemy = world.objects.CreateEnemy(spawnInfo);
            if (enemy == null) return;
            enemy.position.Value = position;
            world.objects.SpawnObject(enemy);
        }
    }
}
