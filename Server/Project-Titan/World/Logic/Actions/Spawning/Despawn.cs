using System;
using System.Collections.Generic;
using System.Text;
using World.Logic.Reader;
using World.Logic.States;
using World.Map.Objects.Entities;

namespace World.Logic.Actions.Spawning
{
    public class DespawnValue
    {

    }

    public class Despawn : LogicAction<DespawnValue>
    {
        public override bool ReadParameterValue(string name, LogicScriptReader reader)
        {
            return false;
        }

        public override void Init(Entity enemy, out DespawnValue obj, ref StateContext context, ref WorldTime time)
        {
            obj = null;
            enemy.world.objects.RemoveObjectPostLogic(enemy);
        }

        public override void Tick(Entity entity, ref DespawnValue obj, ref StateContext context, ref WorldTime time)
        {

        }
    }
}
