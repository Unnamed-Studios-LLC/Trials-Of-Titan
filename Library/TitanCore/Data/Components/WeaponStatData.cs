using System;
using TitanCore.Core;
using Utils.NET.IO.Xml;

namespace TitanCore.Data.Components
{
    public class WeaponStatData
    {
        /// <summary>
        /// The type of stat this object defines
        /// </summary>
        public WeaponStatType type;

        /// <summary>
        /// The value that this stat starts at
        /// </summary>
        public int baseValue;

        /// <summary>
        /// The maximum value that this stat can have
        /// </summary>
        public int maxValue;

        public WeaponStatData(XmlParser xml)
        {
            Parse(xml);
        }

        /// <summary>
        /// Parses xml data
        /// </summary>
        /// <param name="xml"></param>
        public void Parse(XmlParser xml)
        {
            type = xml.AtrEnum("type", WeaponStatType.Damage);
            baseValue = xml.Int("Base");
            maxValue = xml.Int("Max");
        }
    }
}
