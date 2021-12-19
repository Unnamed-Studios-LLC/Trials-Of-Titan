using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using TitanCore.Data;
using Utils.NET.Utils;
using World.Logic.Reader;
using World.Logic.States;
using World.Map.Objects.Entities;

namespace World.Looting
{
    public class Single : Loot, ILootable
    {
        //private Item item = new Item(0, false, 1);
        private Item[] lootItems;

        public Single(int chance, params Item[] lootItems) : base(chance)
        {
            this.lootItems = lootItems;
        }

        public void AddItems(List<Item> items, PlayerLootVariables variables)
        {
            if (!DoChance(variables.damagePercent)) return;
            items.Add(lootItems.Random());
        }
    }
}
