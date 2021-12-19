using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Data.Components.Projectiles;
using TitanCore.Data.Items;
using TitanCore.Net;
using UnityEngine;

public class ItemAoeProjectile : AoeProjectile
{
    public void Setup(IEnumerable<WorldObject> hitGroup, uint time, AoeProjectileData aoeData, Vector2 position, Vector2 target, int damage, uint projId, bool collides, ItemInfo itemInfo)
    {
        Setup(hitGroup, time, aoeData, position, target, damage, projId, collides);

        SetSprite(TextureManager.GetDisplaySprite(itemInfo));
        AddRotation(-360 * 2);
    }
}
