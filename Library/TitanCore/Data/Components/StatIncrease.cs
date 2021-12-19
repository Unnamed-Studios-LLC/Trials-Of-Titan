using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using Utils.NET.IO.Xml;

namespace TitanCore.Data.Components
{
    public class StatIncrease
    {
        /// <summary>
        /// The type of stat this object defines
        /// </summary>
        public StatType type;

        /// <summary>
        /// The amount to increase the stat by
        /// </summary>
        public int amount;

        public StatIncrease(XmlParser xml)
        {
            Parse(xml);
        }

        /// <summary>
        /// Parses xml data
        /// </summary>
        /// <param name="xml"></param>
        public void Parse(XmlParser xml)
        {
            type = xml.AtrEnum("type", StatType.Speed);
            amount = xml.IntValue;
        }
    }
}
