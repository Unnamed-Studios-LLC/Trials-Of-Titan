using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using Utils.NET.IO.Xml;

namespace TitanCore.Data.Items
{
    public class ScrollInfo : ItemInfo
    {
        public override GameObjectType Type => GameObjectType.Scroll;

        public StatType statType;

        public override void Parse(XmlParser xml)
        {
            base.Parse(xml);

            statType = xml.Enum("StatType", StatType.Speed);
        }
    }
}
