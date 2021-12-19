using System;
using System.Collections.Generic;
using System.Linq;
using Utils.NET.Logging;
using Utils.NET.Utils;
using World.Logic.Actions;
using World.Logic.Reader;
using World.Map.Objects.Entities;

namespace World.Logic.States
{
    public class StateValue
    {
        /// <summary>
        /// The values associated with the states
        /// </summary>
        public object[] actionValues;

        /// <summary>
        /// The state context of this state
        /// </summary>
        public StateContext context;

        public bool wasActive = false;
    }

    public class State : LogicAction<StateValue>
    {
        /// <summary>
        /// The name of this state
        /// </summary>
        public string name;
        
        /// <summary>
        /// The actions ran by this state
        /// </summary>
        private LogicAction[] actions;

        public override bool ReadParameterValue(string name, LogicScriptReader reader)
        {
            switch (name)
            {
                case "name":
                    this.name = reader.ReadString();
                    return true;
                case "actions":
                    actions = reader.ReadActions<LogicAction>().ToArray();
                    return true;
            }
            return false;
        }


        public override void Init(Entity entity, out StateValue obj, ref StateContext context, ref WorldTime time)
        {
            obj = new StateValue(); // init value

            obj.context = new StateContext(); // init context
            obj.context.parentContext = context; // set parent context

            if (actions == null) return; // no child actions, ignore creating value array
            obj.actionValues = new object[actions.Length];
        }

        public override void Tick(Entity entity, ref StateValue obj, ref StateContext context, ref WorldTime time)
        {
            if (context.currentState == null) context.currentState = name; // current state is undefined, set it to this state
            else if (context.currentState != name) // do not tick if the state value is incorrent
            {
                obj.wasActive = false;
                return;
            }

            if (obj.actionValues == null) return; // no child values, return..
            for (int i = 0; i < obj.actionValues.Length; i++)
            {
                if (!obj.wasActive)
                {
                    actions[i].Init(entity, out var val, ref obj.context, ref time);
                    obj.actionValues[i] = val;
                }
                actions[i].Tick(entity, ref obj.actionValues[i], ref obj.context, ref time); // tick child actions
            }
            obj.wasActive = true;
        }

        public bool InState(string[] stateNames, int currentIndex, ref object value)
        {
            var stateValue = (StateValue)value;
            var context = stateValue.context;
            if (context.currentState == null || !context.currentState.Equals(stateNames[currentIndex], StringComparison.Ordinal))
                return false;
            currentIndex++;
            if (currentIndex >= stateNames.Length)
                return true;

            if (!TryGetSubState(stateNames[currentIndex - 1], ref stateValue, out var subState, out var subValue))
                return false;
            return subState.InState(stateNames, currentIndex, ref subValue);
        }

        private bool TryGetSubState(string name, ref StateValue stateValue, out State subState, out object value)
        {
            for (int i = 0; i < actions.Length; i++)
            {
                var action = actions[i];
                if (!(action is State state)) continue;
                if (state.name != name) continue;
                subState = state;
                value = stateValue.actionValues[i];
                return true;
            }
            subState = null;
            value = null;
            return false;
        }
    }
}
