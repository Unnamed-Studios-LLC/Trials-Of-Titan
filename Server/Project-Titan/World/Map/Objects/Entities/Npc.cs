using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Data;
using World.Logic;

namespace World.Map.Objects.Entities
{
    public class Npc : NotPlayable
    {
        public override GameObjectType Type => GameObjectType.Npc;

        protected virtual ushort DefaultBehavior => 0;

        /// <summary>
        /// The logic that this enemy runs
        /// </summary>
        private NpcLogic logic;

        public override void Initialize(GameObjectInfo info)
        {
            base.Initialize(info);

            logic = new NpcLogic(this, DefaultBehavior);
        }

        protected override void DoTick(ref WorldTime time)
        {
            base.DoTick(ref time);

            logic.Tick(ref time);
        }

        public override void Hurt(int damage, Entity damager)
        {

        }
    }
}
