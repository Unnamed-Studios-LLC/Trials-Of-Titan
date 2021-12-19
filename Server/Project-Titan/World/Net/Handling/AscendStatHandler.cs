using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using TitanCore.Data.Entities;
using TitanCore.Net.Packets.Client;
using TitanCore.Net.Packets.Models;
using TitanDatabase.Models;
using World.Map.Objects.Map;

namespace World.Net.Handling
{
    public class AscendStatHandler : ClientPacketHandler<TnAscendStat>
    {
        public override void Handle(TnAscendStat packet, Client connection)
        {
            if (!connection.player.world.objects.TryGetObject(packet.tableGameId, out var obj)) return;
            if (!(obj is AscensionTable ascensionTable)) return;
            if (connection.player.DistanceTo(ascensionTable) > 4)
            {
                connection.player.AddChat(ChatData.Error("You are not close enough to the ascension table"));
                return;
            }

            var baseStat = connection.player.GetStatBase(packet.statType);
            var statLock = connection.player.GetStatLock(packet.statType);
            var cost = StatFunctions.GetAscensionCost(packet.statType, baseStat, statLock, out Item itemCost);

            if (cost <= 0)
            {
                return;
            }


            int itemCount = 0;
            for (int i = 4; i < 12; i++) // check item costs
            {
                var serverItem = connection.player.GetItem(i);
                if (serverItem == null) continue;
                var item = serverItem.itemData;
                if (item.id != itemCost.id) continue;
                itemCount += item.count;
            }

            if (itemCount < itemCost.count)
            {
                connection.player.AddChat(ChatData.Error($"You do not have the required scrolls to ascend your {packet.statType} stat!"));
                return;
            }

            var currentSouls = connection.player.fullSouls.Value;

            if (currentSouls < cost)
            {
                connection.player.AddChat(ChatData.Error("Not enough essence to perform level up!"));
                return;
            }

            for (int i = 4; i < 12; i++) // remove item cost
            {
                var serverItem = connection.player.GetItem(i);
                if (serverItem == null) continue;
                var item = serverItem.itemData;
                if (item.id != itemCost.id) continue;

                if (item.count < itemCost.count)
                {
                    itemCost.count -= item.count;
                    connection.player.SetItem(i, null);
                    DeleteItem(serverItem);
                }
                else if (item.count == itemCost.count)
                {
                    connection.player.SetItem(i, null);
                    DeleteItem(serverItem);
                    break;
                }
                else
                {
                    item.count -= itemCost.count;
                    serverItem.itemData = item;
                    connection.player.SetItem(i, serverItem);
                    break;
                }
            }

            connection.player.RemoveFullSouls((ulong)cost);

            connection.player.SetStatBase(packet.statType, baseStat + (packet.statType == StatType.MaxHealth ? 10 : 1));

            connection.player.LeveledUp();

            connection.player.AddChat(ChatData.Info($"Ascended {packet.statType}"));
        }

        private async void DeleteItem(ServerItem item)
        {
            await ServerItem.Delete(item.id);
        }
    }
}
