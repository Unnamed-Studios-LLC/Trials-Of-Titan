using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TitanCore.Core;
using TitanCore.Data;
using TitanCore.Net.Packets.Models;
using TitanDatabase.Models;
using World.GameState;
using World.Map.Objects.Entities;
using World.Map.Objects.Interfaces;
using World.Map.Objects.Map.Containers;
using World.Models;

namespace World.Map.Objects.Map.Containers
{
    public class Vault : GameObject, IContainer
    {
        public override GameObjectType Type => GameObjectType.VaultChest;

        private static Item[] defaultItems = new Item[0];

        public override bool Ticks => false;

        private Account account;

        private ObjectStat<Item[]> items = new ObjectStat<Item[]>(ObjectStatType.VaultData, ObjectStatScope.Public, defaultItems, defaultItems);

        public Vault()
        {

        }

        public Vault(Account account)
        {
            this.account = account;
            items.Value = GetItemArray();
        }

        private Item[] GetItemArray()
        {
            var items = new Item[account.vaultItems.Count];
            for (int i = 0; i < items.Length; i++)
                items[i] = account.vaultItems[i]?.itemData ?? Item.Blank;
            return items;
        }

        protected override void GetStats(List<ObjectStat> list)
        {
            base.GetStats(list);

            list.Add(items);
        }

        public int GetContainerSize()
        {
            return account.vaultIds.Count;
        }

        public ServerItem GetItem(int slot)
        {
            slot = Math.Max(0, Math.Min(GetContainerSize(), slot));
            return account.vaultItems[slot];
        }

        public ulong GetOwnerId()
        {
            return account.id;
        }

        public SlotType GetSlotType(int slot)
        {
            return SlotType.Generic;
        }

        public void SetItem(int slot, ServerItem item)
        {
            slot = Math.Max(0, Math.Min(GetContainerSize(), slot));
            if (item == null)
            {
                account.vaultItems[slot] = null;
                account.vaultIds[slot] = 0;
            }
            else
            {
                item.containerId = account.id;
                account.vaultItems[slot] = item;
                account.vaultIds[slot] = item.id;
            }
            items.Value = GetItemArray();
        }

        public void AddVaultSlot()
        {
            account.vaultIds.Add(0);
            account.vaultItems.Add(null);
            items.Value = GetItemArray();
        }

        public void ReloadItems()
        {
            items.Value = GetItemArray();
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
