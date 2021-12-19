using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO.Xml;

namespace TitanCore.Data.Map
{
    public class StaticObjectInfo : GameObjectInfo
    {
        public override GameObjectType Type => GameObjectType.StaticObject;

        public bool collidable;

        /// <summary>
        /// If this object blocks sight
        /// </summary>
        public bool blockSight = false;

        public override void Parse(XmlParser xml)
        {
            base.Parse(xml);

            collidable = xml.Exists("Collidable");
            blockSight = xml.Exists("BlockSight");
        }
    }
}
