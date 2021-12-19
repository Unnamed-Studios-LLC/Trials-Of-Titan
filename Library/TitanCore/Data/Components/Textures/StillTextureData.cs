using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO.Xml;

namespace TitanCore.Data.Components.Textures
{
    public class StillTextureData : TextureData
    {
        public override TextureType Type => TextureType.Still;

        public override void Parse(XmlParser xml)
        {
            base.Parse(xml);

            displaySprite = xml.String("Sprite", "");
        }

        public StillTextureData()
        {

        }

        public StillTextureData(string sprite)
        {
            displaySprite = sprite;
        }
    }
}
