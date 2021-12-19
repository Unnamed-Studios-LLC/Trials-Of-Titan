using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using TitanCore.Net.Packets.Models;
using World.Map.Objects.Entities;

namespace World.Commands
{
    public class TradeCommand : CommandHandler
    {
        public override Rank MinRank => Rank.Player;

        public override string Command => "trade";

        public override string Syntax => "/trade {playerName}";

        public override ChatData Handle(Player player, CommandArgs args)
        {
            if (args.args.Length == 0) return SyntaxError;

            if (player.GetTradingWith() != null)
                return ChatData.Error("You are already in a trade");

            var playerName = args.args[0];

            if (!player.world.objects.TryGetPlayer(playerName, out var otherPlayer))
                return ChatData.Error($"Unable to find player '{playerName}'");

            if (otherPlayer == player)
                return ChatData.Error($"You are unable to trade with yourself");

            player.SendTradeRequest(otherPlayer);
            if (player.GetTradingWith() == otherPlayer)
                return ChatData.Info("Started trade with " + otherPlayer.playerName.Value);
            else
                return ChatData.Info("Requested to trade with " + otherPlayer.playerName.Value);
        }
    }
}
