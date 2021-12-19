using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Core;
using TitanCore.Data;
using UnityEngine;

public interface IContainer
{
    Vector3 Position { get; }

    bool ShowLootMenu();

    uint GetGameId();

    GameObjectInfo GetInfo();

    World GetWorld();

    Item GetItem(int index);

    void SetItem(int slot, Item item);

    SlotType GetSlotType(int index);

    void SetOnInventoryUpdated(Action action);

    void RemoveOnInventoryUpdated(Action action);
}
