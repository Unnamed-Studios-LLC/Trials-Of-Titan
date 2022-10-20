using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils.NET.IO.Xml;

namespace TitanCore.Data.Map
{
    public class Object3dInfo : GameObjectInfo
    {
        public override GameObjectType Type => GameObjectType.Object3d;

        /// <summary>
        /// All possible meshes for this object. A mesh is randomly picked from the given list to present
        /// </summary>
        public string[] meshNames;

        /// <summary>
        /// The health of the object, if destructable
        /// </summary>
        public int health;

        /// <summary>
        /// If this object blocks sight
        /// </summary>
        public bool blockSight = false;

        public Object3dInfo() : base()
        {

        }

        public override void Parse(XmlParser xml)
        {
            base.Parse(xml);

            meshNames = xml.Elements("Mesh").Select(_ => _.stringValue).ToArray();
            health = xml.Int("Health", 0);
            if (meshNames.Length == 0)
                meshNames = new string[] { name };
            blockSight = xml.Exists("BlockSight");
        }
    }
}
