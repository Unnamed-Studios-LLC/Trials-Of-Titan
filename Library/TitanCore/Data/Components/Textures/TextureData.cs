using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO.Xml;
using Utils.NET.Utils;

namespace TitanCore.Data.Components.Textures
{
    public enum TextureType
    {
        Still,
        Sequence,
        Character,
        Entity
    }

    public abstract class TextureData

    {
        private static TypeFactory<TextureType, TextureData> factory = new TypeFactory<TextureType, TextureData>(_ => _.Type);

        public static TextureData Create(XmlParser xml)
        {
            var type = xml.Enum("Type", TextureType.Still);
            var texture = factory.Create(type);
            texture.Parse(xml);
            return texture;
        }

        public abstract TextureType Type { get; }

        public string displaySprite;

        public string effect;

        public EffectPosition effectPosition;

        public virtual void Parse(XmlParser xml)
        {
            effect = xml.String("Effect");
            effectPosition = xml.Enum("EffectPosition", EffectPosition.Bottom);
        }
    }
}
