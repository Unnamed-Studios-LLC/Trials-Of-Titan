using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Core;
using TitanCore.Data;
using TitanCore.Data.Map;
using TitanCore.Net.Packets.Models;
using UnityEngine;
using Utils.NET.Geometry;

public class Container : SpriteWorldObject, IContainer
{
    public override GameObjectType ObjectType => GameObjectType.Container;

    public Item[] items;

    private bool inventoryUpdated = false;

    public event Action onInventoryUpdated;

    public override void LoadObjectInfo(GameObjectInfo info)
    {
        base.LoadObjectInfo(info);

        var texture = info.textures[UnityEngine.Random.Range(0, info.textures.Length)];
        SetSprite(TextureManager.GetSprite(texture.displaySprite));
    }

    public override void Enable()
    {
        base.Enable();

        transform.localEulerAngles = new Vector3(0, 0, 0);
        items = new Item[8];
        inventoryUpdated = false;
        onInventoryUpdated = null;
    }

    public override void Disable()
    {
        base.Disable();

        LeanTween.cancel(gameObject);
    }

    protected override float GetRelativeScale()
    {
        return 0.9f;
    }

    public override void NetUpdate(NetStat[] stats, bool first)
    {
        base.NetUpdate(stats, first);

        if (inventoryUpdated)
            onInventoryUpdated?.Invoke();
    }

    protected override void ProcessStat(NetStat stat, bool first)
    {
        base.ProcessStat(stat, first);

        switch (stat.type)
        {
            case ObjectStatType.Inventory0:
            case ObjectStatType.Inventory1:
            case ObjectStatType.Inventory2:
            case ObjectStatType.Inventory3:
            case ObjectStatType.Inventory4:
            case ObjectStatType.Inventory5:
            case ObjectStatType.Inventory6:
            case ObjectStatType.Inventory7:
                SetItem((int)stat.type - (int)ObjectStatType.Inventory0, (Item)stat.value);
                inventoryUpdated = true;
                break;
        }
    }

    public override void SetPosition(Vec2 position, bool first)
    {
        base.SetPosition(position, first);

        CheckIfLiquid(position.ToVector2());
    }

    private void CheckIfLiquid(Vector2 position)
    {
        if (!this) return;
        if (gameObject == null || !gameObject) return;
        var tile = world.tilemapManager.GetTileType((int)position.x, (int)position.y);
        if (!GameData.objects.TryGetValue(tile, out var info) || !(info is TileInfo tileInfo)) return;
        if (!tileInfo.liquid) return;

        var seq = LeanTween.sequence();
        seq.append(transform.LeanMoveLocalZ(tileInfo.sink, 1).setEaseInOutSine());
        seq.append(transform.LeanMoveLocalZ(0, 1).setEaseInOutSine());
        seq.append(gameObject, LiquidBobCallback);
    }

    private void LiquidBobCallback()
    {
        CheckIfLiquid(Position);
    }

    public void SetItem(int slot, Item item)
    {
        items[slot] = item;
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

    public Item GetItem(int index)
    {
        return items[index];
    }

    public SlotType GetSlotType(int index)
    {
        return SlotType.Generic;
    }

    public void SetOnInventoryUpdated(Action action)
    {
        onInventoryUpdated += action;
    }

    public void RemoveOnInventoryUpdated(Action action)
    {
        onInventoryUpdated -= action;
    }

    public bool ShowLootMenu()
    {
        return true;
    }
}
