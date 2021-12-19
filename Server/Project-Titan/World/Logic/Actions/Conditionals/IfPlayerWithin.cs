using System;
using System.Collections.Generic;
using System.Text;
using World.Logic.Reader;
using World.Logic.States;
using World.Map.Objects.Entities;

namespace World.Logic.Actions.Conditionals
{
    public class IfPlayerWithin : Conditional<object>
    {
        private float distance;

        public override bool ReadParameterValue(string name, LogicScriptReader reader)
        {
            switch (name)
            {
                case "distance":
                    distance = reader.ReadFloat();
                    return true;
            }
            return base.ReadParameterValue(name, reader);
        }
        protected override void Init(Entity entity, out object obj, ref StateContext context, ref WorldTime time)
        {
            obj = new object();
        }

        protected override bool Condition(Entity entity, ref object obj, ref StateContext context, ref WorldTime time)
        {
            var player = entity.GetClosestPlayer(distance);
            return player != null;
        }
    }
}
