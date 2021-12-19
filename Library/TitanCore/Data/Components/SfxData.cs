using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO.Xml;

namespace TitanCore.Data.Components
{
    public enum SfxType
    {
        Shoot,
        Hit,
        Spawn
    }

    public class SfxData
    {
        /// <summary>
        /// The type of sfx
        /// </summary>
        public SfxType type;

        /// <summary>
        /// The name of the sound to play
        /// </summary>
        public string soundName;

        public SfxData(XmlParser xml)
        {
            Parse(xml);
        }

        /// <summary>
        /// Parses xml data
        /// </summary>
        /// <param name="xml"></param>
        public void Parse(XmlParser xml)
        {
            type = xml.AtrEnum("type", SfxType.Shoot);
            soundName = xml.StringValue;
        }
    }
}
