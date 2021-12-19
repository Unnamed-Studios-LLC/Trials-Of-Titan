using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using Utils.NET.IO.Xml;

namespace TitanCore.Data.Components
{
    public class ClassRequirement
    {
        public ClassType classType;

        public int questRequirement;

        public ClassRequirement(XmlParser xml)
        {
            Parse(xml);
        }

        /// <summary>
        /// Parses xml data
        /// </summary>
        /// <param name="xml"></param>
        public void Parse(XmlParser xml)
        {
            classType = xml.AtrEnum("class", ClassType.Ranger);
            questRequirement = xml.intValue;
        }
    }
}
