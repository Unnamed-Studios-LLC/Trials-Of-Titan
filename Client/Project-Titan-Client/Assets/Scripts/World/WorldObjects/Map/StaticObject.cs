using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Data;
using TitanCore.Data.Map;
using UnityEngine;

public class StaticObject : SpriteWorldObject
{
    public override GameObjectType ObjectType => GameObjectType.StaticObject;

    public override void LoadObjectInfo(GameObjectInfo info)
    {
        base.LoadObjectInfo(info);

        var texture = UnityEngine.Random.Range(0, info.textures.Length);
        SetTexture(texture);

        var staticInfo = (StaticObjectInfo)info;

        
    }

    public override void Enable()
    {
        base.Enable();

        transform.localEulerAngles = new Vector3(0, 0, 0);
    }
}