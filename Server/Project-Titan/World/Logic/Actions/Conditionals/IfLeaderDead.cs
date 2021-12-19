using System;
using System.Collections.Generic;
using System.Text;
using World.Logic.States;
using World.Map.Objects.Entities;

namespace World.Logic.Actions.Conditionals
{
    public class IfLeaderDeadValue { }

    public class IfLeaderDead : Conditional<IfLeaderDeadValue>
    {

        protected override void Init(Entity entity, out IfLeaderDeadValue obj, ref StateContext context, ref WorldTime time)
        {
            obj = new IfLeaderDeadValue();
        }

        protected override bool Condition(Entity entity, ref IfLeaderDeadValue obj, ref StateContext context, ref WorldTime time)
        {
            if (!(entity is Enemy enemy))
                return false;
            return enemy.leader == null || enemy.leader.IsDead;
        }
    }
}
