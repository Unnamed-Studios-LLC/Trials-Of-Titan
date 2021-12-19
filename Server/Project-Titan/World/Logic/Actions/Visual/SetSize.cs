using System;
using System.Collections.Generic;
using System.Text;
using World.Logic.Reader;
using World.Logic.States;
using World.Map.Objects.Entities;

namespace World.Logic.Actions.Visual
{
    public class SetSizeValue
    {

    }

    public class SetSize : LogicAction<SetSizeValue>
    {
        private float value;

        private float rate = 0;

        public override bool ReadParameterValue(string name, LogicScriptReader reader)
        {
            switch (name)
            {
                case "value":
                    value = reader.ReadFloat();
                    return true;
                case "rate":
                    rate = reader.ReadFloat();
                    return true;
            }
            return false;
        }

        public override void Init(Entity entity, out SetSizeValue obj, ref StateContext context, ref WorldTime time)
        {
            obj = new SetSizeValue();
        }

        public override void Tick(Entity entity, ref SetSizeValue obj, ref StateContext context, ref WorldTime time)
        {
            if (rate == 0)
                entity.SetSize(value);
            else
            {
                var step = rate * (float)time.deltaTime;
                var dif = value - entity.GetSize();

                if (Math.Abs(dif) < step)
                    entity.SetSize(value);
                else
                    entity.SetSize(entity.GetSize() + step * (dif < 0 ? -1 : 1));
            }
        }
    }
}
