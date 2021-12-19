using System;
using System.Collections.Generic;
using System.Text;
using World.Logic.Reader;
using World.Logic.States;
using World.Map.Objects.Entities;

namespace World.Logic.Actions
{
    public class DoNothing : LogicAction
    {
        public override bool ReadParameterValue(string name, LogicScriptReader reader)
        {
            return false;
        }

        public override void Init(Entity entity, out object obj, ref StateContext context, ref WorldTime time)
        {
            obj = null;
        }

        public override void Tick(Entity entity, ref object obj, ref StateContext context, ref WorldTime time)
        {

        }
    }
}
