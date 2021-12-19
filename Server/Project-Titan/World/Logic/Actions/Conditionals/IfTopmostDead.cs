using System;
using System.Collections.Generic;
using System.Text;
using World.Logic.States;
using World.Map.Objects.Entities;

namespace World.Logic.Actions.Conditionals
{
    public class IfTopmostDeadValue
    {

    }

    public class IfTopmostDead : Conditional<IfTopmostDeadValue>
    {
        protected override void Init(Entity entity, out IfTopmostDeadValue obj, ref StateContext context, ref WorldTime time)
        {
            obj = new IfTopmostDeadValue();
        }

        protected override bool Condition(Entity entity, ref IfTopmostDeadValue obj, ref StateContext context, ref WorldTime time)
        {
            if (!(entity is Enemy enemy)) return false;
            var topmost = enemy.GetTopmostLeader(out var count);
            return topmost.IsDead;
        }
    }
}
