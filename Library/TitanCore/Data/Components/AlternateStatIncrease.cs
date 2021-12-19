using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using Utils.NET.IO.Xml;

namespace TitanCore.Data.Components
{
    public class AlternateStatIncrease
    {
        /// <summary>
        /// The type of stat this object defines
        /// </summary>
        public AlternateStatType type;

        /// <summary>
        /// The amount to increase the stat by
        /// </summary>
        public int amount;

        public AlternateStatIncrease(XmlParser xml)
        {
            Parse(xml);
        }

        /// <summary>
        /// Parses xml data
        /// </summary>
        /// <param name="xml"></param>
        public void Parse(XmlParser xml)
        {
            type = xml.AtrEnum("type", AlternateStatType.RateOfFire);
            amount = xml.intValue;
        }
    }
}
