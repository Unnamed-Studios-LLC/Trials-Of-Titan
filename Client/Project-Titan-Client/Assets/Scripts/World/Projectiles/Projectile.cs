using System.Collections;
using System.Collections.Generic;
using TitanCore.Data;
using TitanCore.Data.Components.Projectiles;
using TitanCore.Data.Map;
using TitanCore.Net.Packets.Client;
using UnityEngine;
using Utils.NET.Geometry;

public class Projectile : MonoBehaviour
{
    public World world;

    private float angleSin;
    private float angleCos;

    public float angle;

    public ProjectileData data;

    private ProjectileInfo info;

    private float startTimeClient;
    public uint startTime;
    private uint deathTime;

    public Vector2 startPosition;
    private Vector2 lastPosition;

    public uint projId;

    private ObjectShadow shadow;

    private IEnumerable<WorldObject> hitGroup;

    private HashSet<uint> hitIds = new HashSet<uint>();

    public bool players = false;

    public bool enemyOwned = false;

    public float radius = 0.25f;

    public ushort damage;

    public void Setup(IEnumerable<WorldObject> hitGroup, Vector2 position, ProjectileData data, float angle, uint projId, uint time, ushort damage, bool reach)
    {
        this.hitGroup = hitGroup;
        this.data = data;
        this.damage = damage;
        info = (ProjectileInfo)GameData.GetObjectByName(data.infoName);
        enemyOwned = false;
        players = false;

        startPosition = position;
        lastPosition = position;
        angleSin = Mathf.Sin(angle);
        angleCos = Mathf.Cos(angle);
        this.angle = angle;

        float reachTime = 0;
        if (reach && (data.Type == ProjectileType.Linear || data.Type == ProjectileType.Sin))
        {
            reachTime = 1f / data.speed;
        }

        deathTime = time + (uint)((data.lifetime + reachTime) * 1000);
        startTime = time;
        startTimeClient = Time.time;
        this.projId = projId;

        transform.localPosition = new Vector3(position.x, position.y, -0.375f);
        transform.localEulerAngles = new Vector3(0, 0, angle * Mathf.Rad2Deg + info.rotation);
        SetSize(data.size);

        GetComponent<SpriteRenderer>().sprite = TextureManager.GetSprite(info.textures[0].displaySprite);

        shadow = world.gameManager.objectManager.GetShadow(transform);
        shadow.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);

        AngleToPath(GetPosition(0), GetPosition(0.05f));

        hitIds.Clear();
    }

    public void SetSize(float size)
    {
        radius = size * 0.25f;
        transform.localScale = new Vector3(0.8f, 0.8f, 0.8f) * size;
    }

    public void WorldFixedUpdate(uint time)
    {
        if (time > deathTime)
        {
            DestroyProjectile();
            return;
        }

        var newPosition = GetPosition((time - startTime) / 1000f);
        float angle = AngleToPath(lastPosition, newPosition);
        lastPosition = newPosition;

        if (!data.ignoreCollision && world.collision.ProjectileCollides(newPosition.x, newPosition.y))
        {
            if (!enemyOwned)
            {
                var obj = world.tilemapManager.GetObject((int)newPosition.x, (int)newPosition.y);
                if (obj != null && obj.info is Object3dInfo obj3dInfo && obj3dInfo.health > 0)
                {
                    int damageTaken = damage;
                    if (players)
                    {
                        obj.ShowPlayerDamageAlert(damageTaken);
                        world.gameManager.client.SendAsync(new TnHitWall(world.clientTickId, projId, (ushort)newPosition.x, (ushort)newPosition.y));
                    }
                    else
                        obj.ShowAlert("-" + damageTaken, Color.red);
                }
            }
            else
                world.gameManager.client.SendAsync(new TnEnemyHitWall(world.clientTickId, projId));

            DestroyProjectile();
            DoWallHitEffect(angle);
            return;
        }

        if (Hit(new Vec2(newPosition.x, newPosition.y), out var hitObj, out var destroy))
        {
            //if (hitObj is SpriteWorldObject swo)
            //    DoEntityHitEffect(swo, angle);
            if (destroy)
                DestroyProjectile();
        }
    }

    private void LateUpdate()
    {
        if (world != null && world.stopTick) return;
        var newPosition = GetPosition(Time.time - startTimeClient);
        transform.localPosition = new Vector3(newPosition.x, newPosition.y, -0.375f);

        ApplySpin();
    }

    private void ApplySpin()
    {
        if (data.spin == 0) return;
        transform.localEulerAngles += new Vector3(0, 0, Time.deltaTime * 360 * data.spin);
    }

    private Vector2 GetPosition(float time)
    {
        return startPosition + data.GetPosition(time, angleSin, angleCos, projId).ToVector2();
    }

    private float AngleToPath(Vector2 from, Vector2 to)
    {
        Vector2 angleVector = to - from;
        float angle = Mathf.Atan2(angleVector.y, angleVector.x) * Mathf.Rad2Deg;
        if (!data.noPath && data.spin == 0)
            transform.localEulerAngles = new Vector3(0, 0, angle + info.rotation);
        return angle;
    }

    private bool Hit(Vec2 position, out WorldObject wo, out bool destroy)
    {
        foreach (var obj in hitGroup)
        {
            if (!hitIds.Contains(obj.gameId) && obj.IsHitBy(position, this, out var killed))
            {
                hitIds.Add(obj.gameId);
                if (obj is SpriteWorldObject swo)
                    DoEntityHitEffect(swo, angle);

                wo = obj;
                destroy = (!killed || !data.fallthrough) && !data.ignoreEntity;
                return true;
            }
        }
        wo = null;
        destroy = false;
        return false;
    }

    private void DestroyProjectile()
    {
        world.gameManager.objectManager.ReturnProjectile(this);
        world.gameManager.objectManager.ReturnShadow(shadow);
    }

    private void DoWallHitEffect(float angle)
    {
        var effect = (ProjectileCollision)world.PlayEffect(EffectType.ProjectileCollision, transform.localPosition);
        var metaData = TextureManager.GetMetaData(GetComponent<SpriteRenderer>().sprite);
        effect.colors = metaData.colors;
        var angles = effect.transform.localEulerAngles;
        angles.z = -90 + angle;// Mathf.Asin(angleSin) * Mathf.Rad2Deg;
        effect.transform.localEulerAngles = angles;
    }

    private void DoEntityHitEffect(SpriteWorldObject entity, float angle)
    {
        var pos = entity.Position;
        pos.z = transform.localPosition.z;

        var effect = (ProjectileCollision)world.PlayEffect(EffectType.EntityCollision, pos);
        var metaData = TextureManager.GetMetaData(entity.spriteRenderer.sprite);
        effect.colors = metaData.colors;
        //var angles = effect.transform.localEulerAngles;
        //angles.z = -90 + angle;
        effect.transform.localEulerAngles = new Vector3(-90, 0, 0);
    }
}