using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO.Xml;

namespace TitanCore.Data.Map
{
    public class MusicPlayerInfo : StaticObjectInfo
    {
        public override GameObjectType Type => GameObjectType.MusicPlayer;

        public string music;

        public float musicRadius;

        public float worldMusicMin;

        public override void Parse(XmlParser xml)
        {
            base.Parse(xml);

            music = xml.String("Music");
            musicRadius = xml.Float("MusicRadius");
            worldMusicMin = xml.Float("WorldMusicMin", 0.2f);
        }
    }
}
