using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Net;
using TitanCore.Net.Packets.Client;
using TitanCore.Net.Packets.Models;

namespace World.Net.Handling
{
    public class PurchaseVaultSlotHandler : ClientPacketHandler<TnPurchaseVaultSlot>
    {
        public override void Handle(TnPurchaseVaultSlot packet, Client connection)
        {
            if (connection.account.vaultIds.Count >= NetConstants.Max_Vault_Slots)
            {
                connection.player.AddChat(ChatData.Error($"You've reached the maximum amount of vault slots!"));
                return;
            }

            if (connection.account.premiumCurrency < NetConstants.Vault_Slot_Cost)
            {
                connection.player.AddChat(ChatData.Error($"Not enough {NetConstants.Premium_Currency_Name}s to purchase a vault slot!"));
                return;
            }

            var vault = connection.player.GetVault(packet.vaultGameId);
            if (vault == null || vault.DistanceTo(connection.player) > 2)
            {
                connection.player.AddChat(ChatData.Error($"No nearby vault found"));
                return;
            }

            connection.account.premiumCurrency -= NetConstants.Vault_Slot_Cost;
            connection.player.premiumCurrency.Value = connection.account.premiumCurrency;

            vault.AddVaultSlot();

            connection.player.AddChat(ChatData.Info($"Successfully purchased a vault slot!"));
        }
    }
}
