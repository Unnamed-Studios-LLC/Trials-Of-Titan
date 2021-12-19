using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Core;
using TitanCore.Data;
using TitanCore.Data.Components.Projectiles;
using TitanCore.Data.Components.Textures;
using TitanCore.Data.Entities;
using TitanCore.Net;
using TitanCore.Net.Packets.Client;
using TitanCore.Net.Packets.Models;
using UnityEngine;
using Utils.NET.Geometry;

public class Enemy : NotPlayable
{
    public override GameObjectType ObjectType => GameObjectType.Enemy;

    protected override bool Ally => false;

    public int health = 100;

    public int maxHealth;

    public int defense;

    private bool killed = false;

    public override void Enable()
    {
        base.Enable();

        health = 100;
        radius = 0.4f;
        killed = false;
    }

    public override void LoadObjectInfo(GameObjectInfo info)
    {
        base.LoadObjectInfo(info);

        var enemyInfo = (EnemyInfo)info;
        defense = enemyInfo.defense;

        if (enemyInfo.titan)
        {
            indicator.spriteRenderer.sprite = TextureManager.GetDisplaySprite(info);
            indicator.spriteRenderer.color = Color.white;
            indicator.sizeAdjustment = 0.3f;
        }
        else
        {
            indicator.sizeAdjustment = 1;
        }
    }

    protected override float GetRelativeScale()
    {
        return 0.9f;
    }

    protected override void ProcessStat(NetStat stat, bool first)
    {
        base.ProcessStat(stat, first);

        switch (stat.type)
        {
            case ObjectStatType.Health:
                health = (int)stat.value;
                break;
            case ObjectStatType.MaxHealth:
                maxHealth = (int)stat.value;
                break;
            case ObjectStatType.Defense:
                defense = (int)stat.value;
                break;
            case ObjectStatType.Heal:
                ShowAlert("+" + (int)stat.value, Color.green);
                break;
        }
    }

    public void Shoot(EnemyAoeProjectile enemyProj, uint time)
    {
        var enemyInfo = (EnemyInfo)info;
        var projData = enemyInfo.projectiles[enemyProj.index];
        var aoeData = (AoeProjectileData)projData;

        var aoe = (AoeProjectile)world.PlayEffect(EffectType.AoeProjectile, Position);
        aoe.Setup(world.enemyHittables, time, aoeData, Position, enemyProj.target.ToVector2(), enemyProj.damage, enemyProj.projectileId, false);

        world.AddAoeProjectile(aoe);

        if (!projData.passive)
            shootCooldown = 380;
    }

    public void Shoot(EnemyProjectile enemyProj, uint time)
    {
        var enemyInfo = (EnemyInfo)info;
        var projData = enemyInfo.projectiles[enemyProj.index];

        uint projId = enemyProj.projectileId;
        foreach (var angle in NetConstants.GetProjectileAngles(enemyProj.angle, projData.angleGap, projData.amount))
        {
            var proj = world.gameManager.objectManager.GetProjectile();
            proj.Setup(world.enemyHittables, enemyProj.position.ToVector2(), projData, angle, projId++, time, enemyProj.damage, false);
            proj.enemyOwned = true;
        }

        if (!projData.passive)
            shootCooldown = 380;
    }

    public override bool IsHitBy(Vec2 position, Projectile projectile, out bool killed)
    {
        if (this.killed)
        {
            killed = true;
            return false;
        }

        bool hit = base.IsHitBy(position, projectile, out killed);
        if (hit)
        {
            if (!HasStatusEffect(StatusEffect.Invulnerable))
            {
                var damageTaken = GetDamageTaken(projectile.damage);

                if (projectile.players)
                {
                    if (world.player != null && gameId == world.player.target)
                    {
                        world.player.ConsumeTarget();
                        damageTaken *= 2;
                    }

                    ShowPlayerDamageAlert(damageTaken);
                    world.gameManager.client.SendAsync(new TnHit(world.clientTickId, projectile.projId, gameId, Vec2.zero));
                    health -= damageTaken;
                    killed = health <= 0;
                    this.killed = killed;
                    world.player.AddRage();

                    //Debug.Log($"ProjId: {projectile.projId}, Time: {world.clientTime}, Stopped: {stopped}, EnemyPos: {GetPosition().ToVec2()}, Start: {startPosition.ToVec2()}, Target: {targetPosition.ToVec2()}");
                }
                else
                    ShowAlert("-" + damageTaken, Color.red);
            }
        }
        return hit;
    }

    public override void HitBy(AoeProjectile projectile)
    {
        base.HitBy(projectile);

        if (killed) return;
        if (IsInvincible() || HasStatusEffect(StatusEffect.Invulnerable)) return;

        var damageTaken = GetDamageTaken(projectile.damage);

        if (projectile.players)
        {
            ShowPlayerDamageAlert(damageTaken);
            world.gameManager.client.SendAsync(new TnHit(world.clientTickId, projectile.projId, gameId, Vec2.zero));
            health -= damageTaken;
            if (health <= 0)
                killed = true;
            world.player.AddRage();
        }
        else
            ShowAlert("-" + damageTaken, Color.red);
    }

    public override void WorldFixedUpdate(uint time, uint delta)
    {
        base.WorldFixedUpdate(time, delta);

        shootCooldown -= (int)delta;
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();

        if (killed)
            world.RemoveObject(this);
    }

    public override string GetName()
    {
        if (info is EnemyInfo enemyInfo)
            return enemyInfo.title;
        return info.name;
    }

    public override int GetDamageTaken(int damage)
    {
        return StatFunctions.DamageTaken(defense, damage, HasStatusEffect(StatusEffect.Fortified));
    }
}
