using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Data;

namespace World.Map.Objects.Map
{
    public class AscensionTable : GameObject
    {
        public override GameObjectType Type => GameObjectType.AscensionTable;

        public override bool Ticks => false;
    }
}
