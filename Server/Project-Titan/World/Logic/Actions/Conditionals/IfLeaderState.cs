using System;
using System.Collections.Generic;
using System.Text;
using World.Logic.Reader;
using World.Logic.States;
using World.Map.Objects.Entities;

namespace World.Logic.Actions.Conditionals
{
    public class IfLeaderStateValue
    {

    }

    public class IfLeaderState : Conditional<IfLeaderStateValue>
    {
        private List<string[]> stateNames = new List<string[]>();

        public override bool ReadParameterValue(string name, LogicScriptReader reader)
        {
            switch (name)
            {
                case "name":
                    stateNames.Add(reader.ReadString().Split(','));
                    return true;
            }

            return base.ReadParameterValue(name, reader);
        }
        protected override void Init(Entity entity, out IfLeaderStateValue obj, ref StateContext context, ref WorldTime time)
        {
            obj = new IfLeaderStateValue();
        }

        protected override bool Condition(Entity entity, ref IfLeaderStateValue obj, ref StateContext context, ref WorldTime time)
        {
            if (!(entity is Enemy enemy)) return false;
            if (enemy.leader == null || stateNames == null) return false;
            foreach (var stateName in stateNames)
                if (enemy.leader.InState(stateName))
                    return true;
            return false;
        }
    }
}
