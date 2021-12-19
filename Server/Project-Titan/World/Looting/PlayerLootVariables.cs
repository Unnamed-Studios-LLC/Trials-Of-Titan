using System;
using System.Collections.Generic;
using System.Text;

namespace World.Looting
{
    public struct PlayerLootVariables
    {
        public ulong ownerId;

        public int damage;

        public float damagePercent;

        public PlayerLootVariables(ulong ownerId, int damage, float damagePercent)
        {
            this.ownerId = ownerId;
            this.damage = damage;
            this.damagePercent = damagePercent;
        }
    }
}
