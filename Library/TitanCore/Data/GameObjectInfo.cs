using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using TitanCore.Data.Components;
using TitanCore.Data.Components.Textures;
using Utils.NET.Collections;
using Utils.NET.IO.Xml;
using Utils.NET.Utils;

namespace TitanCore.Data
{
    public abstract class GameObjectInfo
    {
        /// <summary>
        /// The type of object this is
        /// </summary>
        public abstract GameObjectType Type { get; }

        /// <summary>
        /// The id of this object
        /// </summary>
        public ushort id;

        /// <summary>
        /// A hexadecimal representation of the id
        /// </summary>
        public string HexId
        {
            get => $"0x{id.ToString("X")}";
            set => id = (ushort)StringUtils.ParseHex(value);
        }

        /// <summary>
        /// The name of this object
        /// </summary>
        public string name;

        /// <summary>
        /// Short description of this object
        /// </summary>
        public string description;

        /// <summary>
        /// Sprites used to display this object
        /// </summary>
        public TextureData[] textures;

        /// <summary>
        /// The texture to start at
        /// </summary>
        public int startTexture;

        /// <summary>
        /// The starting size of this object
        /// </summary>
        public Range size;

        /// <summary>
        /// The sound effects of this object
        /// </summary>
        public Dictionary<SfxType, List<SfxData>> soundEffects = new Dictionary<SfxType, List<SfxData>>();

        /// <summary>
        /// If this object is server only
        /// </summary>
        public bool serverOnly;

        public ushort groundObject;

        /// <summary>
        /// Parses xml data into the object
        /// </summary>
        /// <param name="xml"></param>
        public virtual void Parse(XmlParser xml)
        {
            HexId = xml.AtrString("id");
            name = xml.AtrString("name");
            description = xml.String("Description");
            textures = ParseTextures(xml);
            startTexture = xml.Int("StartTexture", 0);

            size = xml.Float("Size", 1);
            size.min = xml.Float("SizeMin", size.min);
            size.max = xml.Float("SizeMax", size.max);

            serverOnly = xml.Exists("ServerOnly");
            groundObject = (ushort)xml.Hex("GroundObject");

            if (textures.Length == 0)
                textures = new TextureData[] { new StillTextureData(name) };

            foreach (var sfx in xml.Elements("Sfx").Select(_ => new SfxData(_)))
            {
                if (!soundEffects.TryGetValue(sfx.type, out var list))
                {
                    list = new List<SfxData>();
                    soundEffects.Add(sfx.type, list);
                }
                list.Add(sfx);
            }
        }

        /// <summary>
        /// Parses sprite info from the node
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        private TextureData[] ParseTextures(XmlParser xml)
        {
            var textures = new List<TextureData>();
            textures.AddRange(xml.Elements("Texture").Select(TextureData.Create));
            textures.AddRange(xml.Elements("Sprite").Select(_ => new StillTextureData(_.stringValue)));
            return textures.ToArray();
        }

        public GameObjectInfo()
        {

        }
    }
}
