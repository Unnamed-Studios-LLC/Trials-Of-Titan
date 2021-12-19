using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using Utils.NET.IO.Xml;

namespace TitanCore.Data.Components
{
    public enum TileChangeAction
    {
        Elapsed,
        Pressure
    }

    public class TileChange
    {
        public TileChangeAction action;

        public ushort tile;

        public float time;

        public TileChange(XmlParser xml)
        {
            Parse(xml);
        }

        /// <summary>
        /// Parses xml data
        /// </summary>
        /// <param name="xml"></param>
        public void Parse(XmlParser xml)
        {
            action = xml.AtrEnum("action", TileChangeAction.Pressure);
            tile = (ushort)xml.Hex("Tile");
            time = xml.Float("Time");
        }
    }
}
