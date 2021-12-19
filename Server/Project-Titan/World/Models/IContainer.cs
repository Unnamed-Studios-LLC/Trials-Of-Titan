using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using TitanDatabase.Models;

namespace World.Models
{
    public interface IContainer
    {
        ServerItem GetItem(int slot);

        void SetItem(int slot, ServerItem item);

        SlotType GetSlotType(int slot);

        ulong GetOwnerId();

        int GetContainerSize();

        long GetEssence();

        void TakeEssence(int amount);

        bool IsEquipSlot(uint slot);
    }
}
