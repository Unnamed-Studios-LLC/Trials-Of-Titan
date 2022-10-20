using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Data.Components;
using TitanCore.Data.Components.Textures;
using Utils.NET.IO.Xml;

namespace TitanCore.Data.Map
{
    public enum TileFrameSelectionType
    {
        Sequence,
        Random
    }

    public class TileInfo : GameObjectInfo
    {
        public override GameObjectType Type => GameObjectType.Tile;

        public float sink;

        public float speed;

        public int blend;

        public bool liquid;

        public string music;

        public string tag;

        public TileChange change;

        public bool animated;

        public float framesPerSecond;

        public TileFrameSelectionType selectionType;

        public int damage;

        public bool noWalk;

        public TileInfo() : base()
        {

        }

        public override void Parse(XmlParser xml)
        {
            base.Parse(xml);

            sink = xml.Float("Sink");
            speed = xml.Float("Speed", 1);
            blend = xml.Int("Blend", 0);
            liquid = xml.Exists("Liquid");
            damage = xml.Int("Damage");
            noWalk = xml.Exists("NoWalk");

            if (xml.Exists("NoBlend"))
                blend = -1;

            music = xml.String("Music", null);
            tag = xml.String("Tag", null);

            foreach (var el in xml.Elements("TileChange"))
                change = new TileChange(el);

            foreach (var el in xml.Elements("Animated"))
            {
                animated = true;
                framesPerSecond = el.AtrFloat("fps");
                selectionType = el.AtrEnum("selection", TileFrameSelectionType.Sequence);
            }
        }
    }
}
