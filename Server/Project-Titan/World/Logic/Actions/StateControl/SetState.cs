using System;
using Utils.NET.Logging;
using World.Logic.Reader;
using World.Logic.States;
using World.Map.Objects.Entities;

namespace World.Logic.Actions.StateControl
{
    public class SetState : LogicAction
    {
        /// <summary>
        /// The name to set the current state to
        /// </summary>
        private string name;

        /// <summary>
        /// The parent to set the state of
        /// </summary>
        private int parent;

        public override bool ReadParameterValue(string name, LogicScriptReader reader)
        {
            switch (name)
            {
                case "name":
                    this.name = reader.ReadString();
                    return true;
                case "parent":
                    parent = reader.ReadInt();
                    return true;
            }
            return false;
        }

        public override void Init(Entity entity, out object obj, ref StateContext context, ref WorldTime time)
        {
            obj = null;
        }

        public override void Tick(Entity entity, ref object obj, ref StateContext context, ref WorldTime time)
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

            c.currentState = name;
        }
    }
}
