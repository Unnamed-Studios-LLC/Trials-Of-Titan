using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using Utils.NET.IO.Xml;

namespace TitanCore.Data.Items
{
    public class EmoteUnlockerInfo : ItemInfo
    {
        public override GameObjectType Type => GameObjectType.EmoteUnlocker;

        public EmoteType emoteType;

        public override void Parse(XmlParser xml)
        {
            base.Parse(xml);

            emoteType = xml.Enum("Emote", EmoteType.None);
        }
    }
}
