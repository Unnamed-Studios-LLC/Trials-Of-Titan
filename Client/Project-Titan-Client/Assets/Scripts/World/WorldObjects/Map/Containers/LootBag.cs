using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Data;
using UnityEngine;

public class LootBag : Container
{
    public override GameObjectType ObjectType => GameObjectType.LootBag;

    public override void Enable()
    {
        base.Enable();


    }

    public override void Disable()
    {
        base.Disable();

        gameObject.LeanCancel();
    }
}
