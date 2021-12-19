using System;
using System.Linq;
using TitanCore.Core;
using Utils.NET.IO.Xml;
using Utils.NET.Utils;

namespace TitanCore.Data.Components
{
    public class StatData
    {
        /// <summary>
        /// The type of stat this object defines
        /// </summary>
        public StatType type;

        /// <summary>
        /// The value that this stat starts at
        /// </summary>
        public int baseValue;

        /// <summary>
        /// The maximum value that this stat can have
        /// </summary>
        public int maxValue;

        public int[] increaseLoop;

        public StatData(XmlParser xml)
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
            baseValue = xml.Int("Base");
            maxValue = xml.Int("Max");
            increaseLoop = StringUtils.ComponentsFromString(xml.String("Increase"), ',', int.Parse).ToArray();
        }
    }
}
