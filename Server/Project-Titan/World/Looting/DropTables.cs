using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using TitanCore.Data.Entities;

namespace World.Looting
{
    public static class DropTables
    {
        private static Dictionary<SoulGroup, List<LootContainer>> lootTables;

        public static void InitTables()
        {
            lootTables = new Dictionary<SoulGroup, List<LootContainer>>()
            {
                { SoulGroup.OceanBeach, new List<LootContainer>
                {
                    new PublicLoot(
                        new Single(Loot.Chance(3), new Item("Healing Spell"))
                        ),
                    new PublicLoot(
                        Tier.Weapon(Loot.Chance(50), ItemTier.Tier1),
                        Tier.Armor(Loot.Chance(40), ItemTier.Tier1),
                        new Single(Loot.Chance(3), new Item("Healing Spell"))
                        ),
                } },

                { SoulGroup.Grasslands, new List<LootContainer>
                {
                    new PublicLoot(
                        new Single(Loot.Chance(4), new Item("Healing Spell"))
                        ),
                    new PublicLoot(
                        Tier.Weapon(Loot.Chance(2), ItemTier.Tier2),
                        Tier.Armor(Loot.Chance(1.3), ItemTier.Tier2),
                        Tier.Accessory(Loot.Chance(1), ItemTier.Tier1),
                        new Single(Loot.Chance(4), new Item("Healing Spell"))
                        ),
                    new PublicLoot(
                        Tier.Weapon(Loot.Chance(40), ItemTier.Tier2),
                        Tier.Armor(Loot.Chance(38), ItemTier.Tier2),
                        Tier.Accessory(Loot.Chance(3), ItemTier.Tier1),
                        new Single(Loot.Chance(4), new Item("Healing Spell"))
                        ),
                    new PublicLoot(
                        Tier.Weapon(Loot.Chance(70), ItemTier.Tier2),
                        Tier.Armor(Loot.Chance(60), ItemTier.Tier2),
                        Tier.Weapon(Loot.Chance(20), ItemTier.Tier3),
                        Tier.Armor(Loot.Chance(18), ItemTier.Tier3),
                        Tier.Accessory(Loot.Chance(8), ItemTier.Tier1),
                        new Single(Loot.Chance(4), new Item("Healing Spell"))
                        ),
                } },

                { SoulGroup.DarkForest, new List<LootContainer>
                {
                    new PublicLoot(
                        new Single(Loot.Chance(5), new Item("Healing Spell"))
                        ),
                    new PublicLoot(
                        Tier.Weapon(Loot.Chance(3.8), ItemTier.Tier3),
                        Tier.Armor(Loot.Chance(3), ItemTier.Tier3),
                        Tier.Accessory(Loot.Chance(1), ItemTier.Tier2),
                        new Single(Loot.Chance(5), new Item("Healing Spell"))
                        ),
                    new PublicLoot(
                        Tier.Weapon(Loot.Chance(36), ItemTier.Tier3),
                        Tier.Armor(Loot.Chance(34), ItemTier.Tier3),
                        Tier.Accessory(Loot.Chance(2), ItemTier.Tier2),
                        new Single(Loot.Chance(5), new Item("Healing Spell"))
                        ),
                    new PublicLoot(
                        Tier.Weapon(Loot.Chance(60), ItemTier.Tier3),
                        Tier.Armor(Loot.Chance(50), ItemTier.Tier3),
                        Tier.Weapon(Loot.Chance(20), ItemTier.Tier4),
                        Tier.Armor(Loot.Chance(18), ItemTier.Tier4),
                        Tier.Accessory(Loot.Chance(6), ItemTier.Tier2),
                        new Single(Loot.Chance(5), new Item("Healing Spell"))
                        ),
                } },

                { SoulGroup.RictornsGate, new List<LootContainer>
                {
                    new PublicLoot(
                        new Single(Loot.Chance(5), new Item("Healing Spell"))
                        ),
                    new PublicLoot(
                        Tier.Weapon(Loot.Chance(5), ItemTier.Tier3),
                        Tier.Armor(Loot.Chance(3.5), ItemTier.Tier3),
                        Tier.Accessory(Loot.Chance(3), ItemTier.Tier2)
                        ),
                    new PublicLoot(
                        Tier.Weapon(Loot.Chance(100), ItemTier.Tier4),
                        Tier.Armor(Loot.Chance(80), ItemTier.Tier4),
                        Tier.Accessory(Loot.Chance(10), ItemTier.Tier2)
                        ),
                    new SoulboundLoot(0.2f, // 3
                        new Single(Loot.Chance(1), new Item("Ceremonial Bow"))
                        ),
                    new SoulboundLoot(1, // 4
                        new Single(Loot.Chance(0.05f), new Item("Ceremonial Bow"))
                        ),
                } },

                { SoulGroup.Desert, new List<LootContainer>
                {
                    new PublicLoot( // 0
                        new Single(Loot.Chance(6), new Item("Healing Spell"))
                        ),
                    new PublicLoot( // 1
                        Tier.Weapon(Loot.Chance(1.2), ItemTier.Tier4),
                        Tier.Armor(Loot.Chance(1), ItemTier.Tier4),
                        Tier.Accessory(Loot.Chance(0.6), ItemTier.Tier2),
                        new Single(Loot.Chance(6), new Item("Healing Spell"))
                        ),
                    new PublicLoot( // 2
                        Tier.Weapon(Loot.Chance(28), ItemTier.Tier4),
                        Tier.Armor(Loot.Chance(24), ItemTier.Tier4),
                        Tier.Accessory(Loot.Chance(2), ItemTier.Tier2),
                        new Single(Loot.Chance(6), new Item("Healing Spell"))
                        ),
                    new PublicLoot( // 3
                        Tier.Weapon(Loot.Chance(26), ItemTier.Tier5),
                        Tier.Armor(Loot.Chance(23), ItemTier.Tier5),
                        Tier.Accessory(Loot.Chance(10), ItemTier.Tier2)
                        ),
                    new SoulboundLoot(0.3f, // 4
                        new Single(Loot.Chance(0.8),
                            new Item("Scroll of Agility"))
                        ),
                } },

                { SoulGroup.Gorge, new List<LootContainer>
                {
                    new PublicLoot( // 0
                        new Single(Loot.Chance(6), new Item("Healing Spell"))
                        ),
                    new PublicLoot( // 1
                        Tier.Weapon(Loot.Chance(1.2), ItemTier.Tier4),
                        Tier.Armor(Loot.Chance(1), ItemTier.Tier4),
                        Tier.Accessory(Loot.Chance(0.6), ItemTier.Tier2),
                        new Single(Loot.Chance(6), new Item("Healing Spell"))
                        ),
                    new PublicLoot( // 2
                        Tier.Weapon(Loot.Chance(20), ItemTier.Tier4),
                        Tier.Armor(Loot.Chance(18), ItemTier.Tier4),
                        Tier.Accessory(Loot.Chance(2), ItemTier.Tier2),
                        new Single(Loot.Chance(6), new Item("Healing Spell"))
                        ),
                    new PublicLoot( // 3
                        Tier.Weapon(Loot.Chance(30), ItemTier.Tier5),
                        Tier.Armor(Loot.Chance(24), ItemTier.Tier5),
                        Tier.Accessory(Loot.Chance(10), ItemTier.Tier2)
                        ),
                    new SoulboundLoot(0.2f, // 4
                        Tier.Weapon(Loot.Chance(40), ItemTier.Tier6),
                        Tier.Armor(Loot.Chance(23), ItemTier.Tier6),
                        Tier.Accessory(Loot.Chance(3), ItemTier.Tier3)
                        ),
                    new SoulboundLoot(0.4f, // 5
                        new Single(Loot.Chance(100),
                            new Item("Scroll of Agility")
                        )
                    ),
                } },

                { SoulGroup.Lake, new List<LootContainer>
                {
                    new PublicLoot( // 0
                        new Single(Loot.Chance(1), new Item("Healing Spell"))
                        ),
                    new PublicLoot( // 1
                        Tier.Weapon(Loot.Chance(1), ItemTier.Tier5),
                        Tier.Armor(Loot.Chance(0.7), ItemTier.Tier5),
                        new Single(Loot.Chance(6), new Item("Healing Spell"))
                        ),
                    new PublicLoot( // 2
                        Tier.Weapon(Loot.Chance(8), ItemTier.Tier5),
                        Tier.Armor(Loot.Chance(6), ItemTier.Tier5),
                        new Single(Loot.Chance(7), new Item("Healing Spell"))
                        ),
                    new PublicLoot( // 3
                        Tier.Weapon(Loot.Chance(20), ItemTier.Tier5),
                        Tier.Armor(Loot.Chance(16), ItemTier.Tier5)
                        ),
                    new SoulboundLoot(0.2f, // 4
                        Tier.Weapon(Loot.Chance(17), ItemTier.Tier6),
                        Tier.Armor(Loot.Chance(14), ItemTier.Tier6),
                        Tier.Accessory(Loot.Chance(8), ItemTier.Tier3),
                        new Single(Loot.Chance(10),
                            new Item("Scroll of Power"))
                        ),
                    new SoulboundLoot(0.3f, // 5
                        new Single(Loot.Chance(0.8),
                            new Item("Scroll of Power"))
                        ),
                } },

                { SoulGroup.Tundra, new List<LootContainer>
                {
                    new PublicLoot( // 0
                        new Single(Loot.Chance(3), new Item("Healing Spell"))
                        ),
                    new SoulboundLoot( // 1
                        Tier.Weapon(Loot.Chance(1.7), ItemTier.Tier6),
                        Tier.Armor(Loot.Chance(1), ItemTier.Tier6),
                        Tier.Accessory(Loot.Chance(1), ItemTier.Tier3)
                        ),
                    new SoulboundLoot( // 2
                        Tier.Weapon(Loot.Chance(4), ItemTier.Tier6),
                        Tier.Armor(Loot.Chance(3), ItemTier.Tier6),
                        Tier.Accessory(Loot.Chance(3), ItemTier.Tier3)
                        ),
                    new SoulboundLoot(0.2f, // 3
                        Tier.Weapon(Loot.Chance(5), ItemTier.Tier7),
                        Tier.Armor(Loot.Chance(4), ItemTier.Tier7),
                        Tier.Accessory(Loot.Chance(5), ItemTier.Tier3),
                        new Single(Loot.Chance(15),
                            new Item("Scroll of Power"))
                        ),
                    new SoulboundLoot(0.3f, // 4
                        new Single(Loot.Chance(1),
                            new Item("Scroll of Stamina"))
                        ),
                } },

                { SoulGroup.Mountains, new List<LootContainer>
                {
                    new PublicLoot( // 0
                        new Single(Loot.Chance(3), new Item("Healing Spell"))
                        ),
                    new SoulboundLoot( // 1
                        Tier.Weapon(Loot.Chance(0.36), ItemTier.Tier6),
                        Tier.Armor(Loot.Chance(0.29), ItemTier.Tier6),
                        Tier.Accessory(Loot.Chance(0.4), ItemTier.Tier3)
                        ),
                    new SoulboundLoot( // 2
                        Tier.Weapon(Loot.Chance(6), ItemTier.Tier6),
                        Tier.Armor(Loot.Chance(4.5), ItemTier.Tier6),
                        Tier.Accessory(Loot.Chance(4), ItemTier.Tier3)
                        ),
                    new SoulboundLoot(0.2f, // 3
                        Tier.Weapon(Loot.Chance(13), ItemTier.Tier7),
                        Tier.Armor(Loot.Chance(10), ItemTier.Tier7),
                        Tier.Accessory(Loot.Chance(5), ItemTier.Tier3)
                        ),
                    new SoulboundLoot(0.3f, // 4
                        new Single(Loot.Chance(100), 
                            new Item("Scroll of Agility"),
                            new Item("Scroll of Power"),
                            new Item("Scroll of Fortitude"),
                            new Item("Scroll of Stamina"))
                        ),
                    new SoulboundLoot(0.3f, // 5
                        new Single(Loot.Chance(1.1),
                            new Item("Scroll of Fortitude"))
                        ),
                } },

                { SoulGroup.ValdoksForge, new List<LootContainer>
                {
                    new PublicLoot( // 0
                        new Single(Loot.Chance(3), new Item("Healing Spell"))
                        ),
                    new SoulboundLoot( // 1
                        Tier.Weapon(Loot.Chance(0.4), ItemTier.Tier6),
                        Tier.Armor(Loot.Chance(0.3), ItemTier.Tier6)
                        ),
                    new SoulboundLoot( // 2
                        Tier.Weapon(Loot.Chance(1), ItemTier.Tier6),
                        Tier.Armor(Loot.Chance(0.8), ItemTier.Tier6)
                        ),
                    new SoulboundLoot(0.1f, // 3
                        Tier.Weapon(Loot.Chance(10), ItemTier.Tier7),
                        Tier.Armor(Loot.Chance(8), ItemTier.Tier7),
                        //Tier.Weapon(Loot.Chance(6), ItemTier.Tier8),
                        //Tier.Armor(Loot.Chance(4), ItemTier.Tier8),
                        Tier.Accessory(Loot.Chance(4), ItemTier.Tier3)
                        ),
                    new SoulboundLoot(0.1f, // 4
                        Tier.Weapon(Loot.Chance(8), ItemTier.Tier8),
                        Tier.Armor(Loot.Chance(6), ItemTier.Tier8),
                        //Tier.Weapon(Loot.Chance(6), ItemTier.Tier9),
                        //Tier.Armor(Loot.Chance(4), ItemTier.Tier9),
                        Tier.Accessory(Loot.Chance(6), ItemTier.Tier4)
                        ),
                    new SoulboundLoot(0.4f, // 5
                        new Single(Loot.Chance(100), new Item("Scroll of Fortitude"))
                        ),
                    new SoulboundLoot(0.8f, // 6
                        new Single(Loot.Chance(100), new Item("Scroll of Fortitude"))
                        ),
                    new SoulboundLoot(0.1f, // 7
                        new Single(Loot.Chance(0.6f), new Item("Valdok's Impervious Aegis"))
                        ),
                    new SoulboundLoot(0.1f, // 8
                        new Single(Loot.Chance(0.8f), new Item("Tehtman's Brutal Band"))
                        ),
                    new SoulboundLoot(0.1f, // 9
                        new Single(Loot.Chance(0.6f), new Item("Bothmur's Zweihander"))
                        ),
                    new SoulboundLoot(0.1f, // 10
                        new Single(Loot.Chance(0.8f), new Item("Ring of Stalwart Vitality"))
                        ),
                    new SoulboundLoot(0.6f, // 11
                        new Single(Loot.Chance(100), new Item("Scroll of Fortitude"))
                        ),
                } },

                { SoulGroup.Dumir, new List<LootContainer>
                {
                    new PublicLoot( // 0
                        new Single(Loot.Chance(3), new Item("Healing Spell"))
                        ),
                    new SoulboundLoot( // 1
                        Tier.Weapon(Loot.Chance(0.4), ItemTier.Tier6),
                        Tier.Armor(Loot.Chance(0.3), ItemTier.Tier6)
                        ),
                    new SoulboundLoot( // 2
                        Tier.Weapon(Loot.Chance(1), ItemTier.Tier6),
                        Tier.Armor(Loot.Chance(0.8), ItemTier.Tier6)
                        ),
                    new SoulboundLoot(0.1f, // 3
                        Tier.Weapon(Loot.Chance(4), ItemTier.Tier7),
                        Tier.Armor(Loot.Chance(2.6), ItemTier.Tier7),
                        //Tier.Weapon(Loot.Chance(6), ItemTier.Tier8),
                        //Tier.Armor(Loot.Chance(4), ItemTier.Tier8),
                        Tier.Accessory(Loot.Chance(3), ItemTier.Tier4)
                        ),
                    new SoulboundLoot(0.1f, // 4
                        Tier.Weapon(Loot.Chance(8), ItemTier.Tier8),
                        Tier.Armor(Loot.Chance(6), ItemTier.Tier8),
                        //Tier.Weapon(Loot.Chance(6), ItemTier.Tier9),
                        //Tier.Armor(Loot.Chance(4), ItemTier.Tier9),
                        Tier.Accessory(Loot.Chance(6), ItemTier.Tier4)
                        ),
                    new SoulboundLoot(0.3f, // 5
                        new Single(Loot.Chance(100), new Item("Scroll of Stamina", false, 1))
                        ),
                    new SoulboundLoot(0.3f, // 6
                        new Single(Loot.Chance(100), new Item("Scroll of Stamina"))
                        ),
                    new SoulboundLoot(0.1f, // 7
                        new Single(Loot.Chance(0.6f), new Item("Oda's Transcendent Longbow"))
                        ),
                    new SoulboundLoot(0.1f, // 8
                        new Single(Loot.Chance(0.8f), new Item("Ring of Sinful Beauty"))
                        ),
                    new SoulboundLoot(0.1f, // 9
                        new Single(Loot.Chance(0.8f), new Item("Arcus's Nimble Circlet"))
                        ),
                    new SoulboundLoot(0.1f, // 10
                        new Single(Loot.Chance(0.6f), new Item("Archmage's Sibylline Vestment"))
                        ),
                    new SoulboundLoot(0.1f, // 11
                        new Single(Loot.Chance(0.6f), new Item("Thumbor"))
                        ),
                    new SoulboundLoot(0.1f, // 12
                        new Single(Loot.Chance(0.8f), new Item("Adorned Band"))
                        ),
                    new SoulboundLoot(0.1f, // 13
                        new Single(Loot.Chance(0.8f), new Item("Ring of the Lonely Spirit"))
                        ),
                } },

                { SoulGroup.MannahsFortress, new List<LootContainer>
                {
                    new PublicLoot( // 0
                        new Single(Loot.Chance(4), new Item("Healing Spell"))
                        ), 
                    new SoulboundLoot( // 1
                        Tier.Weapon(Loot.Chance(0.4), ItemTier.Tier7),
                        Tier.Armor(Loot.Chance(0.3), ItemTier.Tier7)
                        ),
                    new SoulboundLoot( // 2
                        Tier.Weapon(Loot.Chance(1), ItemTier.Tier7),
                        Tier.Armor(Loot.Chance(0.8), ItemTier.Tier7)
                        ),
                    new SoulboundLoot(0.2f, // 3
                        Tier.Weapon(Loot.Chance(10), ItemTier.Tier7),
                        Tier.Armor(Loot.Chance(8), ItemTier.Tier7),
                        Tier.Accessory(Loot.Chance(4), ItemTier.Tier4)
                        ),
                    new SoulboundLoot(0.1f, // 4
                        Tier.Weapon(Loot.Chance(24), ItemTier.Tier8),
                        Tier.Armor(Loot.Chance(18), ItemTier.Tier8),
                        Tier.Weapon(Loot.Chance(5), ItemTier.Tier9),
                        Tier.Armor(Loot.Chance(3), ItemTier.Tier9),
                        Tier.Weapon(Loot.Chance(1), ItemTier.Tier10),
                        //Tier.Armor(Loot.Chance(3), ItemTier.Tier10),
                        Tier.Accessory(Loot.Chance(3), ItemTier.Tier5)
                        ),
                    new SoulboundLoot(0.1f, // 5
                        new Single(Loot.Chance(100),
                            new Item("Scroll of Agility"),
                            new Item("Scroll of Power"),
                            new Item("Scroll of Fortitude"),
                            new Item("Scroll of Stamina"))
                        ),
                    new SoulboundLoot(0.1f, // 6
                        new Single(Loot.Chance(8),
                            new Item("Scroll of Agility"),
                            new Item("Scroll of Power"),
                            new Item("Scroll of Fortitude"),
                            new Item("Scroll of Stamina"))
                        ),
                    new SoulboundLoot(0.1f, // 7
                        new Single(Loot.Chance(0.8f), new Item("Mezhier's Ring of Valor"))
                        ),
                    new SoulboundLoot(0.1f, // 8
                        new Single(Loot.Chance(0.8f), new Item("Mannah's Capstone"))
                        ),
                    new SoulboundLoot(0.1f, // 9
                        new Single(Loot.Chance(0.6f), new Item("Mannah's Soul Crux"))
                        ),
                    new SoulboundLoot(0.1f, // 10
                        new Single(Loot.Chance(0.6f), new Item("Mannah's Mop"))
                        ),
                    new SoulboundLoot(0.1f, // 11
                        new Single(Loot.Chance(0.6f), new Item("Empyrean's Guard"))
                        ),
                    new SoulboundLoot(0.1f, // 12
                        new Single(Loot.Chance(40),
                            new Item("Scroll of Life"))
                        ),
                } },
            };
        }

        public static List<LootContainer> GetLootContainers(EnemyInfo info)
        {
            var containers = new List<LootContainer>();
            if (info.lootTiers.Length == 0) return containers;
            foreach (var lootTier in info.lootTiers)
            {
                if (!lootTables.TryGetValue(info.soulGroup, out var table))
                    continue;
                if (lootTier >= table.Count)
                    continue;
                containers.Add(table[lootTier]);
            }
            return containers;
        }
    }
}
