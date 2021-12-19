using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using TitanCore.Data;
using TitanCore.Net.Packets.Models;
using TitanDatabase.Models;
using World.GameState;
using World.Map.Objects.Entities;
using World.Models;

namespace World.Map.Objects.Map.Containers
{
    public class Container : GameObject, IContainer
    {
        public override GameObjectType Type => GameObjectType.Container;

        public override bool Ticks => true;

        protected ulong ownerId = 0;

        protected Inventory items = new Inventory(8, ObjectStatType.Inventory0, 8);

        protected override void GetStats(List<ObjectStat> list)
        {
            base.GetStats(list);

            list.AddRange(items.stats);
        }

        public override bool CanShowTo(Player player)
        {
            if (ownerId == 0) return true;
            return ownerId == player.GetOwnerId();
        }

        public void SetOwnerId(ulong ownerId)
        {
            this.ownerId = ownerId;
        }

        public int GetContainerSize()
        {
            return items.stats.Length;
        }

        public ServerItem GetItem(int slot)
        {
            return items.GetItem(slot);
        }

        public ulong GetOwnerId()
        {
            return ownerId;
        }

        public SlotType GetSlotType(int slot)
        {
            return SlotType.Generic;
        }

        public void SetItem(int slot, ServerItem item)
        {
            items.SetItem(slot, item);
        }

        public bool TryGiveItem(ServerItem item)
        {
            for (int i = 0; i < GetContainerSize(); i++)
            {
                var o = GetItem(i);
                if (o != null) continue;
                SetItem(i, item);
                return true;
            }
            return false;
        }

        public long GetEssence()
        {
            return 0;
        }

        public void TakeEssence(int amount)
        {

        }

        public bool IsEquipSlot(uint slot)
        {
            return false;
        }
    }
}
