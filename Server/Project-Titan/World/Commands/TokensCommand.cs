using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using TitanCore.Net.Packets.Models;
using World.Map.Objects.Entities;

namespace World.Commands
{
    public class TokensCommand : CommandHandler
    {
        public override Rank MinRank => Rank.Admin;

        public override string Command => "tokens";

        public override string Syntax => "/tokens";

        public override ChatData Handle(Player player, CommandArgs args)
        {
            player.client.account.premiumCurrency = 10000;
            player.premiumCurrency.Value = player.client.account.premiumCurrency;
            return ChatData.Info("Success!");
        }
    }
}
