using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Data;
using World.Logic.Reader;
using World.Map.Objects.Entities;
using World.Map.Objects.Map;

namespace World.Logic.Actions.Death
{
    public class CreateReturnPortal : DeathAction
    {
        public override bool ReadParameterValue(string name, LogicScriptReader reader)
        {
            return false;
        }

        public override void OnDeath(Enemy enemy, Player killer, List<Damager> damagers)
        {
            if (!enemy.world.manager.TryGetReturnWorld(out var returnWorld)) return;
            var portal = new Portal(returnWorld.worldId);
            portal.worldName.Value = returnWorld.WorldName;
            portal.position.Value = enemy.position.Value;
            portal.Initialize(GameData.objects[returnWorld.PreferredPortal]);
            enemy.world.objects.SpawnObject(portal);
        }
    }
}
