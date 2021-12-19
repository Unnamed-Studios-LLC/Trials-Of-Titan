using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using TitanCore.Data;
using TitanCore.Data.Items;
using TitanDatabase;
using TitanDatabase.Models;
using Utils.NET.Geometry;
using Utils.NET.Logging;
using Utils.NET.Utils;
using World.Logic;
using World.Logic.Reader;
using World.Map.Objects.Entities;
using World.Map.Objects.Map.Containers;

namespace World.Looting
{
    public abstract class LootContainer
    {
        private ILootable[] lootables;

        public LootContainer(ILootable[] lootables)
        {
            this.lootables = lootables;
        }

        public abstract void OnDeath(Enemy enemy, Player killer, List<Damager> damagers, Dictionary<ulong, List<Item>> itemBags);

        protected void RunLoot(Enemy enemy, PlayerLootVariables variables, Dictionary<ulong, List<Item>> itemBags)
        {
            if (!itemBags.TryGetValue(variables.ownerId, out var bag))
            {
                bag = new List<Item>();
                itemBags.Add(variables.ownerId, bag);
            }

            foreach (var lootable in lootables)
            {
                lootable.AddItems(bag, variables);
            }
        }
    }
}
