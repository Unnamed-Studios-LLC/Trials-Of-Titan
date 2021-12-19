using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using TitanCore.Data.Entities;
using TitanCore.Data.Items;
using TitanCore.Net;
using TitanCore.Net.Packets.Client;
using TitanCore.Net.Packets.Models;
using TitanDatabase;
using TitanDatabase.Models;
using Utils.NET.Utils;
using World.Models;

namespace World.Net.Handling
{
    public class UseItemHandler : ClientPacketHandler<TnUseItem>
    {
        public override void Handle(TnUseItem packet, Client connection)
        {
            uint time = packet.clientTickId * NetConstants.Client_Delta;

            if (!connection.player.world.objects.TryGetObject(packet.gameId, out var owner)) // retrieve objects
            {
                owner = connection.player.GetVault(packet.gameId);
                if (owner == null)
                    return;
            }

            if (connection.player.DistanceTo(owner) > 1) return; // too far away

            if (!(owner is IContainer container)) // check if containers
            {
                return;
            }

            if (packet.slot >= container.GetContainerSize() || packet.slot < 0) // check container sizes to slot index
            {
                return;
            }

            var ownerId = container.GetOwnerId();

            if (ownerId != 0 && ownerId != connection.account.id) // different owners
            {
                return;
            }

            var serverItem = container.GetItem(packet.slot); // get item

            if (serverItem == null) // item is blank
            {
                return;
            }

            var item = serverItem.itemData;
            var itemInfo = item.GetInfo();

            if (!itemInfo.consumable) return;

            if (itemInfo.heals > 0)
            {
                if (connection.player.gameState.playerState.Health(time) >= connection.player.gameState.playerState.currentSnapshot.GetFunctionalStat(StatType.MaxHealth))
                {
                    connection.player.AddChat(ChatData.Error("You are already at maximum health."));
                    return;
                }
                connection.player.Heal(itemInfo.heals);
            }

            if (itemInfo is ScrollInfo scrollInfo)
            {
                var level = connection.player.GetLevel();
                if (level >= NetConstants.Max_Level)
                {
                    connection.player.AddChat(ChatData.Error($"You've already achieved the max level!"));
                    return;
                }

                if (!connection.player.IncreaseStat(scrollInfo.statType, 1))
                {
                    connection.player.AddChat(ChatData.Error($"{scrollInfo.statType} is already maxed!"));
                    return;
                }
                else
                {
                    connection.player.AddChat(ChatData.Info($"Increased {StringUtils.Labelize(scrollInfo.statType.ToString())} by {(scrollInfo.statType == StatType.MaxHealth ? 10 : 1)}"));
                }
            }
            else if (itemInfo is EmoteUnlockerInfo emoteInfo)
            {
                if (connection.account.HasUnlockedItem(emoteInfo.id))
                {
                    connection.player.AddChat(ChatData.Error($"You've already unlocked the {StringUtils.Labelize(emoteInfo.emoteType.ToString())}' emote!"));
                    return;
                }
                connection.UnlockEmote(emoteInfo);
            }
            else if (itemInfo is PetSpawnerInfo petSpawnInfo)
            {
                connection.player.character.pet = petSpawnInfo.petSpawned;
                connection.player.LoadPet();
            }

            if (item.count > 1)
            {
                item.count -= 1;
                serverItem.itemData = item;
                container.SetItem(packet.slot, serverItem);
            }
            else
            {
                container.SetItem(packet.slot, null);
                Destroy(serverItem);
            }
        }

        private async void Destroy(ServerItem serverItem)
        {
            await ServerItem.Delete(serverItem.id);
        }
    }
}
