using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using TitanCore.Net.Packets.Models;
using World.Map.Objects.Entities;

namespace World.Commands
{
    public class QCommand : CommandHandler
    {
        public override Rank MinRank => Rank.Admin;

        public override string Command => "q";

        public override string Syntax => "/q";

        public override ChatData Handle(Player player, CommandArgs args)
        {
            if (player.quest == null)
                return ChatData.Error("You currently do not have a quest assigned.");

            player.AddEffect(StatusEffect.Invulnerable, 5);
            player.Goto(player.quest.position.Value);
            return ChatData.Info("Teleported to quest.");
        }
    }
}
