using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.Logging;
using World.Logic.Components;
using World.Logic.Reader;
using World.Logic.States;
using World.Map.Objects.Entities;

namespace World.Logic.Actions.Timing
{
    public class TimedSwitchValue
    {
        public int index;

        public object actionValue;

        public object cooldownValue;
    }

    public class TimedSwitch : LogicAction<TimedSwitchValue>
    {
        /// <summary>
        /// The cooldown of the shots
        /// </summary>
        private Cooldown cooldown = new Cooldown();

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
            if (cooldown.ReadParameterValue(name, reader))
                return true;
            return false;
        }

        public override void Init(Entity entity, out TimedSwitchValue obj, ref StateContext context, ref WorldTime time)
        {
            obj = new TimedSwitchValue();
            obj.index = 0;
            cooldown.Init(out obj.cooldownValue);

            var action = actions[obj.index];
            action.Init(entity, out obj.actionValue, ref context, ref time);
        }

        public override void Tick(Entity entity, ref TimedSwitchValue obj, ref StateContext context, ref WorldTime time)
        {
            if (actions.Length == 0) return;

            LogicAction action = null;
            if (cooldown.Tick(ref obj.cooldownValue, ref time))
            {
                obj.index = (obj.index + 1) % actions.Length;
                action = actions[obj.index];
                action.Init(entity, out obj.actionValue, ref context, ref time);
            }
            else
                action = actions[obj.index];
            action.Tick(entity, ref obj.actionValue, ref context, ref time);
        }
    }
}
