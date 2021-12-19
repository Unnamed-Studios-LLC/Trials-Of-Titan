using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Data.Components.Projectiles;
using TitanCore.Net;
using UnityEngine;
using Utils.NET.Geometry;

public class AoeProjectile : Bomb
{
    private uint startTime;

    private uint endTime;

    private uint duration;

    private float angle;

    private bool collides;

    public bool players = false;

    private IEnumerable<WorldObject> hitGroup;

    private Vector2 startPosition;

    private Vector2 target;

    public AoeProjectileData aoeData;

    public int damage;

    public uint projId;

    private Color color;

    public void Setup(IEnumerable<WorldObject> hitGroup, uint time, AoeProjectileData aoeData, Vector2 startPosition, Vector2 target, int damage, uint projId, bool collides)
    {
        this.hitGroup = hitGroup;
        this.startPosition = startPosition;
        this.target = target;
        this.aoeData = aoeData;
        this.damage = damage;
        this.projId = projId;
        this.collides = collides;

        color = aoeData.color.ToUnityColor();
        SetInfo(color, startPosition, target, aoeData.lifetime);
        AddBlast(color, aoeData.radius);

        startTime = time;
        endTime = NetConstants.GetAoeExpireTime(time, World.Fixed_Delta, aoeData.lifetime);
        duration = endTime - startTime;

        angle = startPosition.ToVec2().AngleTo(target.ToVec2()) * Mathf.Rad2Deg;
        players = false;
    }

    protected override bool IsExpired()
    {
        return false;
    }

    public void WorldFixedUpdate(uint time)
    {
        if (collides)
        {
            var newPosition = startPosition + (target - startPosition) * ((time - startTime) / (float)duration);
            if (world.collision.ProjectileCollides(newPosition.x, newPosition.y))
            {
                world.RemoveAoeProjectile(this);
                DoWallHitEffect(angle);
                blastArea = 0;
                Expire();
            }
        }
        if (time < endTime) return;
        world.RemoveAoeProjectile(this);
        Expire();
        CheckHits();
    }

    private void DoWallHitEffect(float angle)
    {
        var effect = (ProjectileCollision)world.PlayEffect(EffectType.ProjectileCollision, transform.localPosition);
        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            var metaData = TextureManager.GetMetaData(spriteRenderer.sprite);
            effect.colors = metaData.colors;
        }
        else
            effect.colors = new Color[] { color };

        var angles = effect.transform.localEulerAngles;
        angles.z = -90 + angle;// Mathf.Asin(angleSin) * Mathf.Rad2Deg;
        effect.transform.localEulerAngles = angles;
    }

    private void CheckHits()
    {
        foreach (var obj in hitGroup)
        {
            if (obj is Object3d)
            {
                if ((((Vector2)obj.transform.localPosition + new Vector2(0.5f, 0.5f)) - target).magnitude < aoeData.radius)
                {
                    obj.HitBy(this);
                }
            }
            else if (obj is Enemy enemy)
            {
                if ((enemy.GetPosition() - target).magnitude < aoeData.radius)
                {
                    obj.HitBy(this);
                    Debug.Log($"Hit! Id: {projId}, projPos: {target.ToVec2()}, enemyPos: {((Vector2)obj.transform.localPosition).ToVec2()}");
                }
            }
            else if (((Vector2)obj.transform.localPosition - target).magnitude < aoeData.radius)
            {
                obj.HitBy(this);
                Debug.Log($"Hit! Id: {projId}, projPos: {target.ToVec2()}, enemyPos: {((Vector2)obj.transform.localPosition).ToVec2()}");
            }
        }
    }
}
