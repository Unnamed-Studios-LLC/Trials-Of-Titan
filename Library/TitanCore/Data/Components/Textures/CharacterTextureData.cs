using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO.Xml;

namespace TitanCore.Data.Components.Textures
{
    public class CharacterTextureData : TextureData
    {
        public override TextureType Type => TextureType.Character;

        /// <summary>
        /// The name of the sprite set
        /// </summary>
        public string spriteSetName;

        public override void Parse(XmlParser xml)
        {
            base.Parse(xml);

            spriteSetName = xml.String("Set");
            displaySprite = spriteSetName + "-1";
        }
    }
}
