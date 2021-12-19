using System;
using System.Collections.Generic;
using System.Text;
using World.Logic.Reader;
using World.Logic.States;
using World.Map.Objects.Entities;

namespace World.Logic.Actions.Movement
{
    public class LinkedPartValue
    {
    }

    public class LinkedPart : LogicAction<LinkedPartValue>
    {

        public override bool ReadParameterValue(string name, LogicScriptReader reader)
        {
            return false;
        }

        public override void Init(Entity entity, out LinkedPartValue obj, ref StateContext context, ref WorldTime time)
        {
            obj = new LinkedPartValue();
        }

        public override void Tick(Entity entity, ref LinkedPartValue obj, ref StateContext context, ref WorldTime time)
        {
            if (!(entity is Enemy enemy)) return;
            var first = enemy.GetTopmostLeader(out var topCount);
            var last = enemy.GetBottomMinion(out var bottomCount);

            var total = topCount + bottomCount;
            float position = bottomCount / (float)total;

            enemy.MoveTo(last.position.Value + (first.position.Value - last.position.Value) * position);
        }
    }
}
