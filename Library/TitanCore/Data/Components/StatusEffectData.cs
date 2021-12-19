using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using Utils.NET.IO.Xml;

namespace TitanCore.Data.Components
{
    public class StatusEffectData
    {
        /// <summary>
        /// The type of status effect to apply
        /// </summary>
        public StatusEffect type;

        /// <summary>
        /// The duration of the status effect
        /// </summary>
        public uint duration;

        public StatusEffectData(XmlParser xml)
        {
            Parse(xml);
        }

        /// <summary>
        /// Parses xml data
        /// </summary>
        /// <param name="xml"></param>
        public void Parse(XmlParser xml)
        {
            type = xml.AtrEnum("type", StatusEffect.Slowed);
            duration = (uint)xml.IntValue;
        }
    }
}
