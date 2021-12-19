using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Data.Items;
using TitanCore.Net;
using TitanCore.Net.Packets.Client;
using TitanCore.Net.Packets.Models;
using TitanDatabase.Models;
using Utils.NET.Logging;
using World.Models;

namespace World.Net.Handling
{
    public class SwapHandler : ClientPacketHandler<TnSwap>
    {
        public override void Handle(TnSwap packet, Client connection)
        {
            if (packet.ownerA == packet.ownerB && packet.slotA == packet.slotB) return;

            if (!connection.player.world.objects.TryGetObject(packet.ownerA, out var ownerA)) // retrieve objects
            {
                ownerA = connection.player.GetVault(packet.ownerA);
                if (ownerA == null)
                    return;
            }
            if (!connection.player.world.objects.TryGetObject(packet.ownerB, out var ownerB))
            {
                ownerB = connection.player.GetVault(packet.ownerB);
                if (ownerB == null)
                    return;
            }

            if (connection.player.GetTradingWith() != null) return;

            if (ownerA.DistanceTo(ownerB) > 1) return; // too far away

            if (!(ownerA is IContainer containerA)) // check if containers
            {
                return;
            }
            if (!(ownerB is IContainer containerB))
            {
                return;
            }

            if (packet.slotA >= containerA.GetContainerSize()) // check container sizes to slot index
            {
                return;
            }
            if (packet.slotB >= containerB.GetContainerSize())
            {
                return;
            }

            var ownerIdA = containerA.GetOwnerId();
            var ownerIdB = containerB.GetOwnerId();

            if (ownerIdA != ownerIdB && ownerIdA != 0 && ownerIdB != 0) // different owners
            {
                return;
            }

            var slotTypeA = containerA.GetSlotType((int)packet.slotA); // get slot types
            var slotTypeB = containerB.GetSlotType((int)packet.slotB);

            var itemA = containerA.GetItem((int)packet.slotA); // get items
            var itemB = containerB.GetItem((int)packet.slotB);

            if (itemA == null && itemB == null) // items are both blank
            {
                return;
            }


            if ((itemA != null && itemA.itemData.soulbound && ownerIdB != ownerIdA) ||
                (itemB != null && itemB.itemData.soulbound && ownerIdA != ownerIdB)) // dont allow sb swap into non sb bag
            {
                return;
            }

            if ((itemA != null && !itemA.itemData.CanSwapInto(slotTypeB)) || (itemB != null && !itemB.itemData.CanSwapInto(slotTypeA))) // slot types wont allow a swap
            {
                return;
            }

            var infoA = itemA == null ? null : itemA.itemData.GetInfo();
            var infoB = itemB == null ? null : itemB.itemData.GetInfo();

            if (itemA != null && itemB != null && itemA.itemData.id == itemB.itemData.id)
            {
                var availableCount = infoB.maxStack - itemB.itemData.count;
                if (availableCount > 0)
                {
                    var stack = itemA.itemData.count + itemB.itemData.count;
                    int extra = 0;
                    if (stack > infoA.maxStack)
                    {
                        extra = stack - infoA.maxStack;
                        stack = infoA.maxStack;
                    }

                    itemB.itemData.count = (byte)stack;
                    if (extra > 0)
                    {
                        itemA.itemData.count = (byte)extra;
                    }
                    else
                    {
                        Destroy(itemA);
                        itemA = null;
                    }

                    containerA.SetItem((int)packet.slotA, itemA);
                    containerB.SetItem((int)packet.slotB, itemB);
                    
                    return;
                }
            }

            if (containerA.IsEquipSlot(packet.slotB) ^ containerB.IsEquipSlot(packet.slotA))
            {
                if (infoA != null && infoA is EquipmentInfo equipA && equipA.soulless && containerB.IsEquipSlot(packet.slotB))
                {
                    var bCurrency = containerB.GetEssence();
                    if (bCurrency < NetConstants.Soulless_Cost_Equip)
                    {
                        connection.player.AddChat(ChatData.Error("You do not have enough essence to equip that!"));
                        return;
                    }
                    containerB.TakeEssence(NetConstants.Soulless_Cost_Equip);
                }

                if (infoB != null && infoB is EquipmentInfo equipB && equipB.soulless && containerA.IsEquipSlot(packet.slotA))
                {
                    var aCurrency = containerA.GetEssence();
                    if (aCurrency < NetConstants.Soulless_Cost_Equip)
                    {
                        connection.player.AddChat(ChatData.Error("You do not have enough essence to equip that!"));
                        return;
                    }
                    containerA.TakeEssence(NetConstants.Soulless_Cost_Equip);
                }
            }

            containerA.SetItem((int)packet.slotA, itemB); // do swap
            containerB.SetItem((int)packet.slotB, itemA);
        }

        private async void Destroy(ServerItem serverItem)
        {
            await ServerItem.Delete(serverItem.id);
        }
    }
}
