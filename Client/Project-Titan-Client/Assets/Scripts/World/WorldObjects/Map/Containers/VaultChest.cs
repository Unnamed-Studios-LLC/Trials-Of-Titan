using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Core;
using TitanCore.Data;
using TitanCore.Net.Packets.Models;
using UnityEngine;

public class VaultChest : SpriteWorldObject, IContainer, IInteractable
{
    public override GameObjectType ObjectType => GameObjectType.VaultChest;

    public string[] InteractionOptions => new string[] { "Open" };

    public string InteractionTitle => "Vault";

    private static Item[] defaultItems = new Item[0];

    public Item[] items = defaultItems;

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
        items = defaultItems;
        inventoryUpdated = false;
        onInventoryUpdated = null;
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
            case ObjectStatType.VaultData:
                items = (Item[])stat.value;
                inventoryUpdated = true;
                break;
        }
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();

        if (((Vector2)world.player.Position - (Vector2)Position).magnitude > 0.7f)
        {
            if (world.gameManager.ui.vaultMenu != null)
                world.gameManager.ui.vaultMenu.Hide();
        }
    }

    public void SetItem(int slot, Item item)
    {
        if (slot >= items.Length) return;
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
        if (index >= items.Length) return Item.Blank;
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

    public void Interact(int option)
    {
        world.gameManager.ui.vaultMenu.Toggle(this);
    }

    public bool ShowLootMenu()
    {
        return false;
    }

    public void OnEnter()
    {

    }

    public void OnExit()
    {

    }
}
