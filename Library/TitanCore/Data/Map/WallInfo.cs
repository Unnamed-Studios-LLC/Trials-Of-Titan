using System;
using System.Linq;
using Utils.NET.IO.Xml;

namespace TitanCore.Data.Map
{
    public class WallInfo : Object3dInfo
    {
        public override GameObjectType Type => GameObjectType.Wall;

        public WallInfo() : base()
        {

        }
    }
}
