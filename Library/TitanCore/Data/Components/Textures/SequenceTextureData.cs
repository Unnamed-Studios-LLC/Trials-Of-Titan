using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils.NET.IO.Xml;

namespace TitanCore.Data.Components.Textures
{
    public class SequenceTextureData : TextureData
    {
        public override TextureType Type => TextureType.Sequence;

        /// <summary>
        /// The set of sprites
        /// </summary>
        public string spriteSetName;

        /// <summary>
        /// The minimun amount of frames per second
        /// </summary>
        public float framesPerSecondMin = 0;

        /// <summary>
        /// The maximum amount of frames per second
        /// </summary>
        public float framesPerSecondMax = -1;

        /// <summary>
        /// The amount of times to loop (-1 == infinite)
        /// </summary>
        public int loopCount = -1;

        /// <summary>
        /// If this sequence should play in reverse order
        /// </summary>
        public bool reverse;

        public bool noFlip;

        public override void Parse(XmlParser xml)
        {
            base.Parse(xml);

            spriteSetName = xml.String("Set");
            displaySprite = spriteSetName + "-" + xml.Int("Display", 1);

            framesPerSecondMin = xml.Float("Fps", 0.25f);

            framesPerSecondMin = xml.Float("FpsMin", framesPerSecondMin);
            framesPerSecondMax = xml.Float("FpsMax", framesPerSecondMax);

            loopCount = xml.Int("Loops", -1);
            reverse = xml.Exists("Reverse");
            noFlip = xml.Exists("NoFlip");
        }
    }
}
