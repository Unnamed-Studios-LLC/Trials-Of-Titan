using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO.Xml;

namespace TitanCore.Data.Map
{
    public class GroundObjectInfo : StaticObjectInfo
    {
        public override GameObjectType Type => GameObjectType.GroundObject;
    }
}
