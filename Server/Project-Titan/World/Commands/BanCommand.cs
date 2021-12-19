using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using TitanCore.Net.Packets.Models;
using World.Map.Objects.Entities;

namespace World.Commands
{
    public class BanCommand : CommandHandler
    {
        public override Rank MinRank => Rank.Mod;

        public override string Command => "ban";

        public override string Syntax => "/ban {playerName} {minutes}";

        public override ChatData Handle(Player player, CommandArgs args)
        {
            if (args.args.Length != 2 || !int.TryParse(args.args[1], out var minutes))
                return SyntaxError;

            if (!player.world.objects.TryGetPlayer(args.args[0], out var otherPlayer))
            {
                return ChatData.Error("Unabled to find player: " + args.args[0]);
            }

            otherPlayer.client.account.bannedUntil = DateTime.UtcNow.AddMinutes(minutes);
            otherPlayer.client.Disconnect();
            return ChatData.Info($"Banned {otherPlayer.playerName.Value} for {minutes} minutes");
        }
    }
}
