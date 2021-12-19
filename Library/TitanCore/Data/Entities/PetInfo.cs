using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO.Xml;

namespace TitanCore.Data.Entities
{
    public class PetInfo : NpcInfo
    {
        public override GameObjectType Type => GameObjectType.Pet;

        public int slots;

        public override void Parse(XmlParser xml)
        {
            base.Parse(xml);

            slots = xml.Int("Slots", 8);
        }
    }
}
