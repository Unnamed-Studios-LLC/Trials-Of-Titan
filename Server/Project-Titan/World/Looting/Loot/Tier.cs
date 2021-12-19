using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TitanCore.Core;
using TitanCore.Data;
using TitanCore.Data.Items;
using Utils.NET.Logging;
using Utils.NET.Utils;
using World.Logic.Reader;
using World.Map.Objects.Entities;

namespace World.Looting
{
    public class Tier : Loot, ILootable
    {
        private ItemTier tier;

        private SlotType[] slotTypes;

        private Item[] lootItems;

        public static Tier Weapon(int chance, ItemTier tier) => new Tier(chance, tier, SlotType.Sword, SlotType.Bow, SlotType.Claymore, SlotType.Spear, SlotType.Elixir, SlotType.Crossbow);
        public static Tier Armor(int chance, ItemTier tier) => new Tier(chance, tier, SlotType.HeavyArmor, SlotType.LightArmor, SlotType.Robe);
        public static Tier Accessory(int chance, ItemTier tier) => new Tier(chance, tier, SlotType.Accessory);

        public Tier(int chance, ItemTier tier, params SlotType[] slotTypes) : base(chance)
        {
            this.slotTypes = slotTypes;
            this.tier = tier;
            SetLootItems();
        }

        private void SetLootItems()
        {
            lootItems = GameData.objects.Values
                .Where(_ => (_ is EquipmentInfo equip) && equip.tier == tier)
                .Select(_ => new Item(_.id, false, 1))
                .Where(_ => slotTypes.Length == 0 || slotTypes.Any(t => _.CanSwapInto(t)))
                .ToArray();
        }

        public void AddItems(List<Item> items, PlayerLootVariables variables)
        {
            if (lootItems.Length == 0) return;
            if (DoChance(variables.damagePercent))
            {
                items.Add(lootItems.Random());
            }
            /*
            foreach (var item in lootItems)
                if (DoChance(variables.damagePercent))
                {
                    items.Add(item);
                }
                */
        }
    }
}
