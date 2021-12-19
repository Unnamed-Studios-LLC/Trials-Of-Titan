using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using TitanCore.Data;
using TitanCore.Data.Items;
using TitanCore.Net.Packets.Client;
using TitanCore.Net.Packets.Models;
using TitanDatabase;
using TitanDatabase.Models;
using World.GameState;
using World.Map.Objects.Entities;
using World.Map.Objects.Interfaces;

namespace World.Map.Objects.Map
{
    public class MarketDisplay : GameObject, IInteractable
    {
        public override GameObjectType Type => GameObjectType.MarketDisplay;

        public override bool Ticks => false;

        public ObjectStat<Item> purchasable = new ObjectStat<Item>(ObjectStatType.Inventory0, ObjectStatScope.Public, Item.Blank, Item.Blank);

        public ObjectStat<long> premiumCost = new ObjectStat<long>(ObjectStatType.PremiumCurrency, ObjectStatScope.Public, (long)0, (long)0);

        public ObjectStat<long> cost = new ObjectStat<long>(ObjectStatType.DeathCurrency, ObjectStatScope.Public, (long)0, (long)0);

        public MarketDisplay(Item item, long premiumCost, long cost)
        {
            purchasable.Value = item;
            this.premiumCost.Value = premiumCost;
            this.cost.Value = cost;

            Initialize(GameData.objects[0xa50]);
        }

        protected override void GetStats(List<ObjectStat> list)
        {
            base.GetStats(list);

            list.Add(purchasable);
            list.Add(premiumCost);
            list.Add(cost);
        }

        public void Interact(Player player, TnInteract interact)
        {
            var tickDif = world.time.tickId - spawnTickId;
            if (tickDif < WorldManager.Ticks_Per_Second)
            {
                player.AddChat(ChatData.Error("Error purchasing, please try again."));
                return;
            }

            if (player.GetTradingWith() != null)
            {
                player.AddChat(ChatData.Error("Unable to purchase while in a trade."));
                return;
            }

            var type = (ObjectStatType)interact.value;
            long purchaseCost = GetCost(type);
            if (type <= 0)
            {
                player.AddChat(ChatData.Error("Unable to process purchase request."));
                return;
            }

            if (!player.HasOpenSlot())
            {
                player.AddChat(ChatData.Error("Your inventory is full, please leave a non-equip slot open."));
                return;
            }

            var info = purchasable.Value.GetInfo();
            var skinUnlocker = info as SkinUnlockerInfo;

            if (player.client.account.HasUnlockedItem(purchasable.Value.id))
            {
                player.AddChat(ChatData.Error("You already own this item!"));

                if (skinUnlocker != null && player.info.id == skinUnlocker.characterType)
                {
                    player.SetSkin(skinUnlocker.id);
                }
                return;
            }

            if (skinUnlocker != null && player.info.id != skinUnlocker.characterType)
            {
                var correctClass = GameData.objects[(ushort)skinUnlocker.characterType];
                player.AddChat(ChatData.Error($"'{info.name}' is only available for the '{correctClass.name}' class."));
                return;
            }

            if (!player.client.TryTakeCurrency(type, purchaseCost))
            {
                player.AddChat(ChatData.Error("You do not have enough currency to complete the purchase!"));
                return;
            }

            if (player.world == null) return;

            if (info.id == 0x2a9) // Beginner's pack
            {
                if (player.client.account.HasUnlockedItem(info.id))
                {
                    player.AddChat(ChatData.Error("You have already purchased the Beginner's Pack."));
                    return;
                }

                player.client.account.UnlockItem(info.id);

                player.client.account.maxCharacters++;
                for (int i = 0; i < 12; i++)
                {
                    player.client.account.vaultIds.Add(0);
                    player.client.account.vaultItems.Add(null);
                }

                var vault = player.GetVault();
                if (vault != null)
                {
                    vault.ReloadItems();
                }

                player.AddChat(ChatData.Info("Successfully purchased the Beginner's Pack"));
                player.AddChat(ChatData.Info("Unlocked 1 Character Slot!"));
                player.AddChat(ChatData.Info("Unlocked 12 Vault Slots!"));
            }
            else if (info is EmoteUnlockerInfo emoteUnlocker)
            {
                player.client.UnlockEmote(emoteUnlocker);
            }
            else if (skinUnlocker != null)
            {
                player.client.UnlockSkin(skinUnlocker);
                player.SetSkin(skinUnlocker.id);
            }
            else
                CreateItem(player.world, type, player.client.account.id, player.character.id);
        }

        private async void CreateItem(World world, ObjectStatType currencyType, ulong accountId, ulong characterId)
        {
            var response = await Database.CreateItem(purchasable.Value, characterId);

            world.PushTickAction(() =>
            {
                if (response.result != CreateItemResult.Success)
                    RefundItem(world, currencyType, accountId);
                else
                    GiveItem(response.item, world, currencyType, accountId);
            });
        }

        private void RefundItem(World world, ObjectStatType currencyType, ulong accountId)
        {
            if (!(world.objects.TryGetPlayer(accountId, out var player)))
                return;

            player.client.GiveCurrency(currencyType, GetCost(currencyType));
        }

        private void GiveItem(ServerItem item, World world, ObjectStatType currencyType, ulong accountId)
        {
            if (!(world.objects.TryGetPlayer(accountId, out var player)))
            {
                return;
            }

            if (!player.TryGiveItem(item))
                RefundItem(world, currencyType, accountId);
            else
                player.AddChat(ChatData.Info($"Successfully purchased 1 '{item.itemData.GetInfo().name}'"));
        }

        private long GetCost(ObjectStatType type)
        {
            switch (type)
            {
                case ObjectStatType.DeathCurrency:
                    return cost.Value;
                case ObjectStatType.PremiumCurrency:
                    return premiumCost.Value;
                default:
                    return 0;
            }
        }
    }
}
