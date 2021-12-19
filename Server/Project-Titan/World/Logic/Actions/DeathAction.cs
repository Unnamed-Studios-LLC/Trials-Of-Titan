using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TitanCore.Core;
using World.Logic.Actions;
using World.Logic.Reader;
using World.Logic.States;
using World.Map.Objects.Entities;

namespace World.Logic
{
    public struct Damager
    {
        public Player player;

        public int damage;

        public Damager(Player player, int damage)
        {
            this.player = player;
            this.damage = damage;
        }
    }

    public abstract class DeathAction : ReadableAction
    {
        /// <summary>
        /// Initializes the action
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="time"></param>
        public abstract void OnDeath(Enemy enemy, Player killer, List<Damager> damagers);
    }
}
