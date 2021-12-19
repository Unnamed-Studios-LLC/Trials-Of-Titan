using System;
using System.Collections.Generic;
using System.Text;
using World.Logic.Reader;
using World.Logic.States;
using World.Map.Objects.Entities;

namespace World.Logic.Actions.Visual
{
    public class SetHoverValue
    {

    }

    public class SetHover : LogicAction<SetHoverValue>
    {
        private float value;

        public override bool ReadParameterValue(string name, LogicScriptReader reader)
        {
            switch (name)
            {
                case "value":
                    value = reader.ReadFloat();
                    return true;
            }
            return false;
        }

        public override void Init(Entity entity, out SetHoverValue obj, ref StateContext context, ref WorldTime time)
        {
            obj = new SetHoverValue();
        }

        public override void Tick(Entity entity, ref SetHoverValue obj, ref StateContext context, ref WorldTime time)
        {
            entity.SetHover(value);
        }
    }
}
