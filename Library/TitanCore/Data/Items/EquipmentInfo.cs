using System;
using System.Collections.Generic;
using System.Linq;
using TitanCore.Core;
using TitanCore.Data.Components;
using Utils.NET.IO.Xml;

namespace TitanCore.Data.Items
{
    public class EquipmentInfo : ItemInfo
    {
        public override GameObjectType Type => GameObjectType.Equipment;

        public Dictionary<StatType, int> statIncreases = new Dictionary<StatType, int>();

        public Dictionary<AlternateStatType, int> alternateStatIncreases = new Dictionary<AlternateStatType, int>();

        public ItemTier tier = ItemTier.Untiered;

        public bool soulless = false;

        public EquipmentInfo() : base()
        {

        }

        public override void Parse(XmlParser xml)
        {
            base.Parse(xml);

            if (Enum.TryParse<ItemTier>(xml.AtrString("tier", "-1"), true, out var result))
                tier = result;

            foreach (var statIncrease in xml.Elements("StatIncrease").Select(_ => new StatIncrease(_)))
            {
                if (!statIncreases.TryGetValue(statIncrease.type, out var amount))
                    amount = 0;
                amount += statIncrease.amount;
                statIncreases[statIncrease.type] = amount;
            }

            foreach (var increase in xml.Elements("AlternateStatIncrease").Select(_ => new AlternateStatIncrease(_)))
            {
                if (!alternateStatIncreases.TryGetValue(increase.type, out var amount))
                    amount = 0;
                amount += increase.amount;
                alternateStatIncreases[increase.type] = amount;
            }

            soulless = xml.Exists("Soulless");
        }

        public string GetTierDisplay()
        {
            switch (tier)
            {
                case ItemTier.Untiered:
                    return "UT";
                case ItemTier.Starter:
                    return "S";
                default:
                    return "T" + (int)tier;
            }
        }
    }
}
