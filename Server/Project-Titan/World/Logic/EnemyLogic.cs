using System;
using System.Collections.Generic;
using System.IO;
using TitanCore.Core;
using TitanCore.Data;
using Utils.NET.Logging;
using World.Logic.Reader;
using World.Logic.States;
using World.Map.Objects.Entities;

namespace World.Logic
{
    public class EnemyLogic
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
        private readonly Enemy enemy;

        /// <summary>
        /// Null context to pass into the state
        /// </summary>
        private StateContext context;

        /// <summary>
        /// If this logic has been initialized
        /// </summary>
        private bool initialized = false;

        public EnemyLogic(Enemy enemy)
        {
            this.enemy = enemy;
            EntityState.states.TryGetValue(enemy.info.id, out state);
        }

        public void Tick(ref WorldTime time)
        {
            if (!initialized)
            {
                state?.Init(enemy, out value, ref context, ref time);
                initialized = true;
            }
            state?.Tick(enemy, ref value, ref context, ref time);
        }

        public void OnDeath(Player killer, List<Damager> damagers)
        {
            state?.OnDeath(enemy, killer, damagers);
        }

        public bool InState(string[] stateNames)
        {
            if (state == null) return false;
            return state.InState(stateNames, 0, ref value);
        }
    }
}
