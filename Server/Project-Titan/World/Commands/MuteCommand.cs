using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using TitanCore.Net.Packets.Models;
using World.Map.Objects.Entities;

namespace World.Commands
{
    public class MuteCommand : CommandHandler
    {
        public override Rank MinRank => Rank.Admin;

        public override string Command => "mute";

        public override string Syntax => "/mute {playerName} {minutes}";

        public override ChatData Handle(Player player, CommandArgs args)
        {
            if (args.args.Length != 2 || !int.TryParse(args.args[1], out var minutes))
                return SyntaxError;

            if (!player.world.objects.TryGetPlayer(args.args[0], out var otherPlayer))
            {
                return ChatData.Error("Unabled to find player: " + args.args[0]);
            }

            otherPlayer.client.account.mutedUntil = DateTime.UtcNow.AddMinutes(minutes);
            return ChatData.Info($"Muted {otherPlayer.playerName.Value} for {minutes} minutes");
        }
    }
}
