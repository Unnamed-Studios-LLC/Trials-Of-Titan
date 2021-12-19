using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Data;
using Utils.NET.Logging;
using World.Logic.Reader;
using World.Logic.States;
using World.Map.Objects.Entities;

namespace World.Logic.Actions.Visual
{
    public class SetGroundObjectValue
    {

    }

    public class SetGroundObject : LogicAction<SetGroundObjectValue>
    {
        private ushort groundObject;

        public override bool ReadParameterValue(string name, LogicScriptReader reader)
        {
            switch (name)
            {
                case "name":
                    var typeName = reader.ReadString();
                    var info = GameData.GetObjectByName(typeName);

                    if (info == null)
                        Log.Error("No object named: " + typeName);
                    groundObject = info.id;
                    return true;
            }
            return false;
        }

        public override void Init(Entity entity, out SetGroundObjectValue obj, ref StateContext context, ref WorldTime time)
        {
            obj = new SetGroundObjectValue();
        }

        public override void Tick(Entity entity, ref SetGroundObjectValue obj, ref StateContext context, ref WorldTime time)
        {
            entity.SetGroundObject(groundObject);
        }
    }
}
