using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO.Xml;

namespace TitanCore.Data.Map
{
    public class LootBagInfo : ContainerInfo
    {
        public override GameObjectType Type => GameObjectType.LootBag;

        public override void Parse(XmlParser xml)
        {
            base.Parse(xml);


        }
    }
}
