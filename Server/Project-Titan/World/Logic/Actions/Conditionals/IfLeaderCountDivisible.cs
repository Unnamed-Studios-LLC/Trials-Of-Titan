using System;
using System.Collections.Generic;
using System.Text;
using World.Logic.Reader;
using World.Logic.States;
using World.Map.Objects.Entities;

namespace World.Logic.Actions.Conditionals
{
    public class IfLeaderCountDivisibleValue
    {
    }

    public class IfLeaderCountDivisible : Conditional<IfLeaderCountDivisibleValue>
    {
        private int divider = 0;

        private int offset = 0;

        public override bool ReadParameterValue(string name, LogicScriptReader reader)
        {
            switch (name)
            {
                case "value":
                    divider = reader.ReadInt();
                    return true;
                case "offset":
                    offset = reader.ReadInt();
                    return true;
            }
            return base.ReadParameterValue(name, reader);
        }

        protected override void Init(Entity entity, out IfLeaderCountDivisibleValue obj, ref StateContext context, ref WorldTime time)
        {
            obj = new IfLeaderCountDivisibleValue();
        }

        protected override bool Condition(Entity entity, ref IfLeaderCountDivisibleValue obj, ref StateContext context, ref WorldTime time)
        {
            if (!(entity is Enemy enemy)) return false;
            return ((enemy.LeaderCount() + offset) % divider) == 0;
        }
    }
}
