using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.Logging;
using World.Logic.States;
using World.Map.Objects.Entities;

namespace World.Logic
{
    public class NpcLogic
    {
        /// <summary>
        /// The value of the entity state
        /// </summary>
        private object value;

        /// <summary>
        /// The entity state
        /// </summary>
        private readonly EntityState state;

        /// <summary>
        /// The enemy to run logic on
        /// </summary>
        private readonly Npc npc;

        /// <summary>
        /// Null context to pass into the state
        /// </summary>
        private StateContext context;

        /// <summary>
        /// If this logic has been initialized
        /// </summary>
        private bool initialized = false;

        public NpcLogic(Npc npc, ushort defaultBehavior = 0)
        {
            this.npc = npc;
            if (!EntityState.states.TryGetValue(npc.info.id, out state) && defaultBehavior != 0)
            {
                EntityState.states.TryGetValue(defaultBehavior, out state);
            }
        }

        public void Tick(ref WorldTime time)
        {
            if (!initialized)
            {
                state?.Init(npc, out value, ref context, ref time);
                initialized = true;
            }
            state?.Tick(npc, ref value, ref context, ref time);
        }
    }
}
