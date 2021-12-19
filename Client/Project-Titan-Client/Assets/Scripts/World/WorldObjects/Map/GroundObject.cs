using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Data;

public class GroundObject : StaticObject
{
    public override GameObjectType ObjectType => GameObjectType.GroundObject;

    protected override bool HasShadow => false;

    protected override bool IsBillboard => false;

    protected override void SetOutlineGlow(WorldDrawStyle style)
    {
        
    }
}
