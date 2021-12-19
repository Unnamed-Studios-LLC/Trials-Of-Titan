using System;
using System.Collections.Generic;
using System.Text;
using World.Logic.Reader;
using World.Logic.States;
using World.Map.Objects.Entities;

namespace World.Logic.Actions.Visual
{
    public class SetTextureValue
    {

    }

    public class SetTexture : LogicAction<SetTextureValue>
    {
        private int index;

        public override bool ReadParameterValue(string name, LogicScriptReader reader)
        {
            switch (name)
            {
                case "index":
                    index = reader.ReadInt();
                    return true;
            }
            return false;
        }

        public override void Init(Entity entity, out SetTextureValue obj, ref StateContext context, ref WorldTime time)
        {
            obj = new SetTextureValue();
        }

        public override void Tick(Entity entity, ref SetTextureValue obj, ref StateContext context, ref WorldTime time)
        {
            entity.SetTexture(index);
        }
    }
}
