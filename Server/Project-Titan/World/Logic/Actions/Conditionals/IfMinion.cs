using System;
using System.Collections.Generic;
using System.Text;
using World.Logic.Reader;
using World.Logic.States;
using World.Map.Objects.Entities;

namespace World.Logic.Actions.Conditionals
{
    public class IfMinionValue
    {

    }

    public class IfMinion : Conditional<IfMinionValue>
    {
        private int leaderCount = 1;

        public override bool ReadParameterValue(string name, LogicScriptReader reader)
        {
            switch (name)
            {
                case "leaderCount":
                    leaderCount = reader.ReadInt();
                    return true;
            }

            return base.ReadParameterValue(name, reader);
        }

        protected override bool Condition(Entity entity, ref IfMinionValue obj, ref StateContext context, ref WorldTime time)
        {
            if (!(entity is Enemy enemy)) return false;
            return enemy.LeaderCount() >= leaderCount;
        }

        protected override void Init(Entity entity, out IfMinionValue obj, ref StateContext context, ref WorldTime time)
        {
            obj = new IfMinionValue();
        }
    }
}
