using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO.Xml;

namespace TitanCore.Data.Components
{
    public class ProjectileRef
    {
        public ushort objectId;

        public int index;

        public ProjectileRef(XmlParser xml)
        {
            Parse(xml);
        }

        /// <summary>
        /// Parses xml data
        /// </summary>
        /// <param name="xml"></param>
        public void Parse(XmlParser xml)
        {
            objectId =(ushort)xml.AtrHex("id");
            index = xml.intValue;
        }
    }
}
