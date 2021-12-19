using System;
using System.Collections.Generic;
using System.Text;
using World.Logic.Reader;
using World.Logic.States;
using World.Map.Objects.Entities;

namespace World.Logic.Actions.StateControl
{
    public class RunValue
    {
        /// <summary>
        /// The values associated with the states
        /// </summary>
        public object[] actionValues;
    }

    public class Run : LogicAction<RunValue>
    {

        /// <summary>
        /// The actions ran by this state
        /// </summary>
        private LogicAction[] actions;

        public override bool ReadParameterValue(string name, LogicScriptReader reader)
        {
            switch (name)
            {
                case "actions":
                    actions = reader.ReadActions<LogicAction>().ToArray();
                    return true;
            }
            return false;
        }

        public override void Init(Entity entity, out RunValue obj, ref StateContext context, ref WorldTime time)
        {
            obj = new RunValue();
            obj.actionValues = new object[actions.Length];
            for (int i = 0; i < actions.Length; i++)
                actions[i].Init(entity, out obj.actionValues[i], ref context, ref time);
        }

        public override void Tick(Entity entity, ref RunValue obj, ref StateContext context, ref WorldTime time)
        {
            for (int i = 0; i < actions.Length; i++)
                actions[i].Tick(entity, ref obj.actionValues[i], ref context, ref time);
        }
    }
}
