using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Data;
using World.Map.Objects;

public class StaticObject : GameObject
{
    public override GameObjectType Type => GameObjectType.StaticObject;

    public override bool Ticks => false;
}