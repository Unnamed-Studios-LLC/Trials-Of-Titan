using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO.Xml;

namespace TitanCore.Data.Items
{
    public class PetSpawnerInfo : ItemInfo
    {
        public override GameObjectType Type => GameObjectType.PetSpawner;

        public ushort petSpawned;

        public override void Parse(XmlParser xml)
        {
            base.Parse(xml);

            petSpawned = (ushort)xml.Hex("Pet", 0);
        }
    }
}
