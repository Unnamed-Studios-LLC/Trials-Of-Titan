using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using Utils.NET.IO.Xml;

namespace TitanCore.Data.Items
{
    public class ItemInfo : GameObjectInfo
    {
        public override GameObjectType Type => GameObjectType.Item;

        public SlotType slotType;

        public bool consumable;

        public int heals;

        public int maxStack;

        public bool unlockable;

        public ItemInfo() : base()
        {

        }

        public override void Parse(XmlParser xml)
        {
            base.Parse(xml);

            slotType = xml.Enum("SlotType", SlotType.Generic);
            consumable = xml.Exists("Consumable");
            heals = xml.Int("Heals");
            maxStack = xml.Int("MaxStack", 1);
            unlockable = xml.Exists("Unlockable");
        }
    }
}
