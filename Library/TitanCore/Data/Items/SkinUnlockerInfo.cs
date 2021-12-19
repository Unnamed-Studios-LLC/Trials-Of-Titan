using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO.Xml;

namespace TitanCore.Data.Items
{
    public class SkinUnlockerInfo : ItemInfo
    {
        public override GameObjectType Type => GameObjectType.SkinUnlocker;

        public uint characterType;

        public override void Parse(XmlParser xml)
        {
            base.Parse(xml);

            characterType = xml.Hex("CharacterType", 1);
        }
    }
}
