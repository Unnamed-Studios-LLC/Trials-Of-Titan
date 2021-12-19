using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using TitanCore.Data;
using TitanCore.Data.Entities;
using TitanCore.Net.Packets.Models;
using TitanDatabase.Models;
using World.GameState;
using World.Models;

namespace World.Map.Objects.Entities
{
    public class Pet : Npc, IContainer
    {
        public override GameObjectType Type => GameObjectType.Pet;

        protected override ushort DefaultBehavior => 0x3000;

        public ulong ownerAccountId;

        public ObjectStat<uint> ownerId = new ObjectStat<uint>(ObjectStatType.OwnerId, ObjectStatScope.Public, (uint)0, (uint)0);

        public Inventory inventory;

        public Pet(ulong ownerAccountId, uint ownerId)
        {
            this.ownerAccountId = ownerAccountId;
            this.ownerId.Value = ownerId;
        }

        public override void Initialize(GameObjectInfo info)
        {
            base.Initialize(info);

            var petInfo = (PetInfo)info;
            inventory = new Inventory(petInfo.slots, ObjectStatType.Backpack0, 0);
        }

        protected override void GetStats(List<ObjectStat> list)
        {
            base.GetStats(list);

            list.Add(ownerId);
        }

        public int GetContainerSize()
        {
            return inventory.Length;
        }

        public ServerItem GetItem(int slot)
        {
            return inventory.GetItem(slot);
        }

        public ulong GetOwnerId()
        {
            return ownerAccountId;
        }

        public SlotType GetSlotType(int slot)
        {
            return SlotType.Generic;
        }

        public void SetItem(int slot, ServerItem item)
        {
            inventory.SetItem(slot, item);
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
