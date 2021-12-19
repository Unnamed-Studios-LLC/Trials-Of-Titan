using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using TitanCore.Net;
using TitanCore.Net.Packets.Models;
using Utils.NET.Utils;
using World.Map.Objects.Entities;

namespace World.Commands
{
    public class GiveTokensCommand : CommandHandler
    {
        public override Rank MinRank => Rank.Admin;

        public override string Command => "givetokens";

        public override string Syntax => "/givetokens {amount}";

        public override ChatData Handle(Player player, CommandArgs args)
        {
            if (args.args.Length == 0)
                return SyntaxError;

            int amount;
            if (!int.TryParse(args.args[0], out amount))
            {
                return ChatData.Error("Failed to parse amount parameter, use syntax: " + Syntax);
            }

            player.client.account.premiumCurrency += amount;
            player.premiumCurrency.Value = player.client.account.premiumCurrency;

            return ChatData.Info($"Successfully gave {amount} {StringUtils.ApplyPlural(NetConstants.Premium_Currency_Name, amount)}");
        }
    }
}
