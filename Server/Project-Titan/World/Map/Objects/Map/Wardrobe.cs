using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Data;
using TitanCore.Data.Items;
using TitanCore.Net.Packets.Client;
using TitanCore.Net.Packets.Models;
using World.Map.Objects.Entities;
using World.Map.Objects.Interfaces;

namespace World.Map.Objects.Map
{
    public class Wardrobe : GameObject, IInteractable
    {
        public override GameObjectType Type => GameObjectType.Wardrobe;

        public override bool Ticks => false;

        public void Interact(Player player, TnInteract interact)
        {
            var skinType = (ushort)interact.value;

            if (skinType == player.info.id)
            {
                player.SetSkin(0);
                return;
            }

            if (!player.client.account.HasUnlockedItem(skinType))
            {
                player.AddChat(ChatData.Error("You have not unlocked that skin!"));
                return;
            }

            if (!GameData.objects.TryGetValue(skinType, out var info) || !(info is SkinUnlockerInfo skinUnlocker) || skinUnlocker.characterType != player.info.id)
            {
                player.AddChat(ChatData.Error("Invalid skin type!"));
                return;
            }

            player.SetSkin(skinType);
        }
    }
}
