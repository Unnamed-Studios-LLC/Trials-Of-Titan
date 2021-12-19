using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO.Xml;

namespace TitanCore.Data.Entities
{
    public abstract class EntityInfo : GameObjectInfo
    {
        public float hover;

        public bool invincible;

        public bool floats;

        public override void Parse(XmlParser xml)
        {
            base.Parse(xml);

            hover = xml.Float("Hover");
            invincible = xml.Exists("Invincible");
            floats = xml.Exists("Floats");
        }
    }
}
