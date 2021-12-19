using UnityEngine;
using System.Collections;
using TitanCore.Net.Packets.Models;
using System;
using TitanCore.Core;
using TitanCore.Data.Items;
using Utils.NET.Geometry;
using TitanCore.Data;
using TitanCore.Data.Map;
using TitanCore.Data.Entities;
using System.Collections.Generic;
using TitanCore.Net;
using TitanCore.Net.Packets.Client;
using TitanCore.Data.Components.Projectiles;

public abstract class Entity : SpriteWorldObject, IContainer
{
    protected abstract bool Ally { get; }

    protected Vec2 newTargetPosition = new Vec2(0, 0);

    protected Vector2 targetPosition = new Vector2(0, 0);

    protected Vector2 startPosition = new Vector2(0, 0);

    protected Vector2 moveVector = new Vector2(0, 0);

    protected uint startMoveTime;

    protected float startMoveTimeVisual;

    protected bool wasStopped = true;

    protected bool stopped = true;

    private bool inventoryUpdated = false;

    private bool positionUpdated = false;

    public event Action onInventoryUpdated;

    private TileInfo lastTile = null;

    //private Int2 lastTilePos = new Int2(-1, -1);
    private ushort lastTileType = 0;

    private uint serverEffects = 0;

    public float radius = 0.3f;

    public override void LoadObjectInfo(GameObjectInfo info)
    {
        base.LoadObjectInfo(info);

        var entityInfo = (EntityInfo)info;
        SetHover(entityInfo.hover);
    }

    public override void Enable()
    {
        base.Enable();

        lastTile = null;
        //lastTilePos = new Int2(-1, -1);
        lastTileType = 0;
        stopped = true;
        wasStopped = true;
        startMoveTime = 0;
        startMoveTimeVisual = Time.time;
        positionUpdated = false;

        newTargetPosition = default;
        targetPosition = new Vector2(0, 0);
        startPosition = new Vector2(0, 0);
        moveVector = new Vector2(0, 0);

        serverEffects = 0;

        inventoryUpdated = false;

        onInventoryUpdated = null;

        radius = 0.3f;
    }

    public override void NetUpdate(NetStat[] stats, bool first)
    {
        positionUpdated = false;
        wasStopped = stopped;

        base.NetUpdate(stats, first);

        if (!positionUpdated)
            stopped = true;

        if ((!wasStopped || !stopped) && !first)
        {
            UpdatePosition();
        }

        if (inventoryUpdated)
            onInventoryUpdated?.Invoke();
    }

    protected override void ProcessStat(NetStat stat, bool first)
    {
        base.ProcessStat(stat, first);

        switch (stat.type)
        {
            case ObjectStatType.StatusEffects:
                serverEffects = (uint)stat.value;
                break;
            case ObjectStatType.Stopped:
                //if ((bool)stat.value)
                //    SetStopped();
                break;
            case ObjectStatType.Inventory0:
            case ObjectStatType.Inventory1:
            case ObjectStatType.Inventory2:
            case ObjectStatType.Inventory3:
            case ObjectStatType.Inventory4:
            case ObjectStatType.Inventory5:
            case ObjectStatType.Inventory6:
            case ObjectStatType.Inventory7:
            case ObjectStatType.Inventory8:
            case ObjectStatType.Inventory9:
            case ObjectStatType.Inventory10:
            case ObjectStatType.Inventory11:
                SetItem((int)stat.type - (int)ObjectStatType.Inventory0, (Item)stat.value);
                inventoryUpdated = true;
                break;
            case ObjectStatType.ServerDamage:
                TakeDamage((int)stat.value);
                break;
        }
    }

    public override void SetPosition(Vec2 position, bool first)
    {
        stopped = (newTargetPosition == position);
        newTargetPosition = position;
        positionUpdated = true;

        if (first)
        {
            targetPosition = newTargetPosition.ToVector2();
            startPosition = targetPosition;
            moveVector = targetPosition - startPosition;
            startMoveTime = world.clientTime;
            startMoveTimeVisual = Time.time;
        }
    }

    private void UpdatePosition()
    {
        startPosition = GetLerpPosition(world.clientTime - startMoveTime, wasStopped);
        targetPosition = newTargetPosition.ToVector2();
        moveVector = targetPosition - startPosition;
        startMoveTime = world.clientTime;
        startMoveTimeVisual = Time.time;
    }

    public virtual Vector2 GetPosition()
    {
        var timeDelta = world.clientTime - startMoveTime;
        return GetLerpPosition(timeDelta, stopped);
    }

    private Vector2 GetLerpPosition(uint timeDelta, bool stopped)
    {
        if (stopped && timeDelta >= 200)
            return targetPosition;
        return startPosition + moveVector * (timeDelta * 0.005f);
    }

    protected virtual Vector2 GetPositionVisual()
    {
        var timeDelta = Time.time - startMoveTimeVisual;
        if (stopped && timeDelta >= 0.2f)
            return targetPosition;
        return startPosition + moveVector * timeDelta * 5f;
    }

    public override void WorldFixedUpdate(uint time, uint delta)
    {
        base.WorldFixedUpdate(time, delta);

        /*
        if (!positionUpdated && lastMoveTick != world.serverTickId)
            SetStopped();
        else
            positionUpdated = false;
        */
    }

    public virtual void TakeDamage(int damageTaken)
    {
        if (damageTaken == 0) return;
        ShowAlert("- " + damageTaken, Color.red);
    }

    protected override void LateUpdate()
    {
        if (world.stopTick) return;

        base.LateUpdate();

        Position = GetPositionVisual();//GetPosition();

        var tilePos = new Int2((int)Position.x, (int)Position.y);
        var tileType = world.tilemapManager.GetTileType(tilePos.x, tilePos.y);
        if (lastTileType != tileType)
        {
            if (tileType != 0)
            {
                var tileInfo = (TileInfo)GameData.objects[tileType];
                if (!((EntityInfo)info).floats)
                    SetSink(tileInfo.sink);

                if (tileInfo.liquid && (lastTile == null || !lastTile.liquid))
                {
                    var splash = (Splash)world.PlayEffect(EffectType.Splash, Position);
                    splash.SetTile(tileInfo);
                }

                SetCurrentTile(tileInfo);
                /*
                if (info != lastTile)
                {
                    SetSink(info.sink);

                    if (info.liquid && !lastTile.liquid)
                    {
                        var splash = (Splash)world.PlayEffect(EffectType.Splash, Position);
                        splash.SetTile(info);
                    }

                    SetCurrentTile(info);
                }
                */
            }
            else
            {
                SetCurrentTile(null);
                SetSink(0);
            }
            lastTileType = tileType;
            //lastTilePos = tilePos;
        }
    }

    protected virtual void SetCurrentTile(TileInfo tileInfo)
    {
        lastTile = tileInfo;
    }

    public virtual Item GetItem(int index)
    {
        return Item.Blank;
    }

    public virtual void SetItem(int index, Item item)
    {

    }

    public virtual bool IsInvincible()
    {
        return ((EntityInfo)info).invincible || HasStatusEffect(StatusEffect.Invincible) || HasStatusEffect(StatusEffect.KnockedBack) || HasStatusEffect(StatusEffect.Grounded);
    }

    public virtual SlotType GetSlotType(int index)
    {
        return SlotType.Generic;
    }

    public override bool IsHitBy(Vec2 position, Projectile projectile, out bool killed)
    {
        killed = false;
        if (IsInvincible()) return false;
        var pos = GetPosition();
        return position.DistanceTo(new Vec2(pos.x, pos.y)) < (projectile.radius + radius * size);
    }

    public virtual int GetDamageTaken(int damage)
    {
        return damage;
    }

    public uint GetGameId()
    {
        return gameId;
    }

    public GameObjectInfo GetInfo()
    {
        return info;
    }

    public World GetWorld()
    {
        return world;
    }

    public void SetOnInventoryUpdated(Action action)
    {
        onInventoryUpdated += action;
    }

    public void RemoveOnInventoryUpdated(Action action)
    {
        onInventoryUpdated -= action;
    }

    public virtual bool HasStatusEffect(StatusEffect effect)
    {
        return ((serverEffects >> (int)effect) & 1) == 1;
    }

    public virtual bool ShowLootMenu()
    {
        return false;
    }
}
