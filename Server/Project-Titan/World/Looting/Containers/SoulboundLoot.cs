using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using TitanCore.Data;
using TitanDatabase;
using TitanDatabase.Models;
using Utils.NET.Geometry;
using Utils.NET.Utils;
using World.Logic;
using World.Logic.Reader;
using World.Map.Objects.Entities;
using World.Map.Objects.Map.Containers;

namespace World.Looting
{
    public class SoulboundLoot : LootContainer
    {
        public float maxPercent = 0.2f;

        public SoulboundLoot(params ILootable[] lootables) : base(lootables)
        {

        }

        public SoulboundLoot(float maxPercent, params ILootable[] lootables) : base(lootables)
        {
            this.maxPercent = maxPercent;
        }

        public override void OnDeath(Enemy enemy, Player killer, List<Damager> damagers, Dictionary<ulong, List<Item>> itemBags)
        {
            foreach (var damager in damagers)
            {
                RunLoot(enemy, new PlayerLootVariables(damager.player.GetOwnerId(), damager.damage, Math.Min((damager.damage / (float)enemy.maxHealth.Value) / maxPercent, 1)), itemBags);
            }
        }
    }
}
