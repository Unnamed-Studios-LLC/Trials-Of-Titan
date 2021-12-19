using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.Logging;
using Utils.NET.Utils;
using World.Logic.Reader;
using World.Logic.States;
using World.Map.Objects.Entities;

namespace World.Logic.Actions.StateControl
{
    public class RandomStateValue
    {

    }

    public class RandomState : LogicAction<RandomStateValue>
    {
        /// <summary>
        /// The possible states to switch to
        /// </summary>
        private List<string> states = new List<string>();

        /// <summary>
        /// The parent state to switch
        /// </summary>
        private int parent = 1;

        public override bool ReadParameterValue(string name, LogicScriptReader reader)
        {
            switch (name)
            {
                case "parent":
                    parent = reader.ReadInt();
                    return true;
                default:
                    states.Add(reader.ReadString());
                    return true;
            }
        }

        public override void Init(Entity enemy, out RandomStateValue obj, ref StateContext context, ref WorldTime time)
        {
            obj = new RandomStateValue();
        }

        public override void Tick(Entity entity, ref RandomStateValue obj, ref StateContext context, ref WorldTime time)
        {
            var c = context;
            for (int i = 0; i < parent; i++)
            {
                c = context.parentContext;
                if (c == null)
                {
                    Log.Error($"[Logic Error][{entity.info.name}] Failed to set state. Parent state at {parent} does not exist");
                    return;
                }
            }

            var state = states[Rand.Next(states.Count)];
            c.currentState = state;
        }
    }
}
