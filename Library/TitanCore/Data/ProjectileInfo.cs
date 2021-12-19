using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO.Xml;

namespace TitanCore.Data
{
    public class ProjectileInfo : GameObjectInfo
    {
        public override GameObjectType Type => GameObjectType.Projectile;

        public float rotation;

        public override void Parse(XmlParser xml)
        {
            base.Parse(xml);

            rotation = xml.Float("Rotation", 0);
        }
    }
}
