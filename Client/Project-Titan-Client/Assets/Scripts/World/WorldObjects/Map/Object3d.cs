using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Data;
using TitanCore.Data.Map;
using TitanCore.Net.Packets.Client;
using UnityEngine;
using Utils.NET.Geometry;

public class Object3d : MeshWorldObject
{
    public override GameObjectType ObjectType => GameObjectType.Object3d;

    public MeshGroup meshGroup;

    private Effect effect;

    public override void LoadObjectInfo(GameObjectInfo info)
    {
        base.LoadObjectInfo(info);

        var obj3dInfo = (Object3dInfo)info;

        meshGroup = MeshManager.GetMesh(obj3dInfo.meshNames[Random.Range(0, obj3dInfo.meshNames.Length)]);
        meshFilter.mesh = meshGroup.mesh;
    }

    public override void Enable()
    {
        base.Enable();

        transform.localEulerAngles = new Vector3(0, 0, 0);

        switch (info.id)
        {
            case 0xa21: // fountain
                effect = world.PlayEffect(EffectType.Fountain, Vector3.zero);
                effect.transform.SetParent(transform);
                effect.transform.localPosition = new Vector3(0.5f, -0.05f, -0.8f);
                effect.transform.localEulerAngles = new Vector3(90, 0, 0);
                break;
        }
    }

    public override void HitBy(AoeProjectile projectile)
    {
        int damageTaken = projectile.damage;
        if (projectile.players)
        {
            ShowPlayerDamageAlert(damageTaken);
            var pos = transform.localPosition;
            world.gameManager.client.SendAsync(new TnHitWall(world.clientTickId, projectile.projId, (ushort)pos.x, (ushort)pos.y));
        }
        else
            ShowAlert("-" + damageTaken, Color.red);
    }

    public override bool IsHitBy(Vec2 position, Projectile projectile, out bool killed)
    {
        var pos = transform.localPosition;
        bool hit = (ushort)pos.x == (ushort)position.x && (ushort)pos.y == (ushort)position.y;
        if (hit)
        {
            int damageTaken = projectile.damage;
            if (projectile.players)
            {
                ShowPlayerDamageAlert(damageTaken);
                world.gameManager.client.SendAsync(new TnHitWall(world.clientTickId, projectile.projId, (ushort)pos.x, (ushort)pos.y));
            }
            else
                ShowAlert("-" + damageTaken, Color.red);
        }
        killed = false;
        return hit;
    }

    public override void Disable()
    {
        base.Disable();

        if (effect != null)
        {
            effect.transform.SetParent(null);
            effect.system.Stop();
            effect = null;
        }
    }
}