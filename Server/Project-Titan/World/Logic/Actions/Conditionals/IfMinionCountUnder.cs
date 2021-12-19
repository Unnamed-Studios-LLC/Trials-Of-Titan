using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Data;
using Utils.NET.Logging;
using World.Logic.Reader;
using World.Logic.States;
using World.Map.Objects.Entities;

namespace World.Logic.Actions.Conditionals
{
    public class IfMinionCountUnderValue
    {

    }

    public class IfMinionCountUnder : Conditional<IfMinionCountUnderValue>//LogicAction<IfMinionCountUnderValue>
    {
        public int count;

        public ushort type = 0;

        public override bool ReadParameterValue(string name, LogicScriptReader reader)
        {
            switch (name)
            {
                case "count":
                    count = reader.ReadInt();
                    break;
                case "name":
                    var typeName = reader.ReadString();
                    var info = GameData.GetObjectByName(typeName);

                    if (info == null)
                        Log.Error("No object named: " + typeName);

                    type = info.id;
                    break;
            }
            return base.ReadParameterValue(name, reader);
        }

        protected override void Init(Entity entity, out IfMinionCountUnderValue obj, ref StateContext context, ref WorldTime time)
        {
            obj = new IfMinionCountUnderValue();
        }

        protected override bool Condition(Entity entity, ref IfMinionCountUnderValue obj, ref StateContext context, ref WorldTime time)
        {
            if (!(entity is Enemy enemy)) return true;
            if (type != 0)
                return enemy.CountMinionsOfType(type) < count;
            else
                return enemy.CountMinions() < count;
        }
    }
}
