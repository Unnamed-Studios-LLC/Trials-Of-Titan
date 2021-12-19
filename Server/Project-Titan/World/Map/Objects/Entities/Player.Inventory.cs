using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using TitanCore.Core;
using TitanCore.Data;
using TitanCore.Data.Entities;
using TitanCore.Data.Items;
using TitanCore.Net;
using TitanCore.Net.Packets.Models;
using TitanDatabase.Models;
using Utils.NET.Geometry;
using Utils.NET.Logging;
using Utils.NET.Utils;
using World.GameState;
using World.Map.Objects.Map.Containers;
using World.Models;

namespace World.Map.Objects.Entities
{
    public partial class Player : IContainer
    {
        /// <summary>
        /// The inventory of the player
        /// </summary>
        private Inventory inventory = new Inventory(12);

        private int itemActions = 0;

        private void LoadInventory()
        {
            for (int i = 0; i < inventory.Length; i++)
            {
                SetItem(i, character.items[i]);
            }
        }

        private void SaveInventory()
        {
            for (int i = 0; i < inventory.Length; i++)
            {
                var item = inventory.GetItem(i);
                if (item == null)
                {
                    character.items[i] = null;
                    character.itemIds[i] = 0;
                }
                else
                {
                    item.containerId = character.id;
                    character.items[i] = item;
                    character.itemIds[i] = item.id;
                }
            }
        }

        /// <summary>
        /// Trys to give this player the given item
        /// </summary>
        /// <param name="item"></param>
        public bool TryGiveItem(ServerItem item)
        {
            for (int i = 4; i < inventory.Length; i++)
            {
                var current = inventory.GetItem(i);
                if (current != null) continue;
                inventory.SetItem(i, item);
                return true;
            }

            return false;
        }

        public ServerItem GetItem(int slot)
        {
            return inventory.GetItem(slot);
        }

        public bool HasOpenSlot()
        {
            for (int i = 4; i < inventory.Length; i++)
            {
                var item = inventory.GetItem(i);
                if (item == null)
                    return true;
            }
            return false;
        }

        public void SetItem(int slot, ServerItem item)
        {
            if (slot < 4)
            {
                var current = GetItem(slot);
                RemoveIncreases(current);

                AddIncreases(item);
            }
            inventory.SetItem(slot, item);
        }

        public SlotType GetSlotType(int slot)
        {
            if (slot > 3) return SlotType.Generic;
            var charInfo = (CharacterInfo)info;
            return charInfo.equipSlots[slot];
        }

        public ulong GetOwnerId()
        {
            return client.account.id;
        }

        public int GetContainerSize()
        {
            return inventory.Length;
        }

        private void RemoveIncreases(ServerItem serverItem)
        {
            if (serverItem == null) return;
            var item = serverItem.itemData;
            var info = item.GetInfo();
            if (!(info is EquipmentInfo equipInfo)) return;
            foreach (var increase in equipInfo.statIncreases)
            {
                if (statIncreases.TryGetValue(increase.Key, out var currentStat))
                {
                    statIncreases[increase.Key] = currentStat - increase.Value;
                    continue;
                }
            }
        }

        private void AddIncreases(ServerItem serverItem)
        {
            if (serverItem == null) return;
            var item = serverItem.itemData;
            var info = item.GetInfo();
            if (!(info is EquipmentInfo equipInfo)) return;
            foreach (var increase in equipInfo.statIncreases)
            {
                if (statIncreases.TryGetValue(increase.Key, out var currentStat))
                {
                    statIncreases[increase.Key] = currentStat + increase.Value;
                    continue;
                }
                statIncreases[increase.Key] = increase.Value;
            }
        }

        public void StartItemAction()
        {
            Interlocked.Increment(ref itemActions);
        }

        public void EndItemAction()
        {
            Interlocked.Decrement(ref itemActions);
        }

        public int GetItemActionCount()
        {
            return Interlocked.CompareExchange(ref itemActions, 0, 0);
        }

        private void TickInventory(ref WorldTime time)
        {
            if (((time.tickId + tickGroup) % (WorldManager.Ticks_Per_Second * 5)) != 0) return; // once every 5 seconds

            for (int i = 0; i < 4; i++)
            {
                var serverItem = GetItem(i);
                if (serverItem == null) continue;
                var data = serverItem.itemData.GetInfo();
                if (data is EquipmentInfo equip && equip.soulless)
                {
                    if (fullSouls.Value < NetConstants.Soulless_Cost_Drain)
                    {
                        UnequipItem(i);
                        AddChat(ChatData.Error($"You ran out of essence to use {data.name}!"));
                        continue;
                    }

                    RemoveFullSouls(NetConstants.Soulless_Cost_Drain);

                    switch (data.id)
                    {
                        case 0x2ab: // soulless robe
                        case 0x2ac: // soulless heavy
                        case 0x2ad: // soulless light
                            if (gameState.playerState == null) break;
                            var maxHp = gameState.playerState.currentSnapshot.GetFunctionalStat(StatType.MaxHealth);
                            Heal((int)((maxHp - gameState.playerState.Health(0)) * 0.1f));
                            break;
                    }
                }
            }
        }

        private void UnequipItem(int slot)
        {
            if (slot >= 4) return;
            var item = GetItem(slot);
            if (item == null) return;

            for (int i = 4; i < 12; i++)
            {
                var otherItem = GetItem(i);
                if (otherItem != null) continue;
                Swap(slot, i);
                return;
            }

            var charInfo = (CharacterInfo)info;

            for (int i = 4; i < 12; i++)
            {
                var otherItem = GetItem(i);
                if (!otherItem.itemData.CanSwapInto(charInfo.equipSlots[slot])) continue;
                if (otherItem.itemData.GetInfo() is EquipmentInfo equip && equip.soulless) continue;
                Swap(slot, i);
                return;
            }

            var bagInfo = GameData.objects[0xf08];
            var bag = new LootBag();
            bag.Initialize(bagInfo);
            bag.position.Value = position.Value + Vec2.FromAngle(Rand.FloatValue() * AngleUtils.PI_2) * 0.4f;
            world.objects.SpawnObject(bag);
            bag.SetOwnerId(GetOwnerId());
            bag.SetItem(0, item);

            SetItem(slot, null);
        }

        private void Swap(int slotA, int slotB)
        {
            var temp = GetItem(slotB);
            SetItem(slotB, GetItem(slotA));
            SetItem(slotA, temp);
        }

        public long GetEssence()
        {
            return fullSouls.Value;
        }

        public void TakeEssence(int amount)
        {
            RemoveFullSouls((ulong)amount);
        }

        public bool IsEquipSlot(uint slot)
        {
            return slot < 4;
        }
    }
}
