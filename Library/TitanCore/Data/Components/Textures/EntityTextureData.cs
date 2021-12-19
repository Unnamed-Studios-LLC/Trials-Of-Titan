using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils.NET.IO.Xml;

namespace TitanCore.Data.Components.Textures
{
    public class EntityTextureData : TextureData
    {
        public override TextureType Type => TextureType.Entity;

        /// <summary>
        /// The name of the sprite set
        /// </summary>
        public string spriteSetName;

        public bool separateWalk = false;

        public bool noAttack = false;

        public bool noWalk = false;

        public bool noFlip = false;

        public override void Parse(XmlParser xml)
        {
            base.Parse(xml);

            spriteSetName = xml.String("Set");
            displaySprite = spriteSetName + "-1";
            separateWalk = xml.Exists("SepWalk");
            noAttack = xml.Exists("NoAttack");
            noWalk = xml.Exists("NoWalk");
            noFlip = xml.Exists("NoFlip");
        }
    }
}
