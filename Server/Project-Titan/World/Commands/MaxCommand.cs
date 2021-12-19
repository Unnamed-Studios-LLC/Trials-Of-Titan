using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using TitanCore.Net.Packets.Models;
using World.Map.Objects.Entities;

namespace World.Commands
{
    public class MaxCommand : CommandHandler
    {
        public override Rank MinRank => Rank.Admin;

        public override string Command => "max";

        public override string Syntax => "/max {max tier}";

        public override ChatData Handle(Player player, CommandArgs args)
        {
            int tier = 5;
            if (args.args.Length != 0 && !int.TryParse(args.args[0], out tier))
                tier = 5;

            if (tier <= 0)
            {
                player.speed.Value = 1;
                player.attack.Value = 1;
                player.defense.Value = 1;
                player.vigor.Value = 1;

                player.maxHealth.Value = 100;
            }
            else
            {
                player.speed.Value = tier * 10;
                player.attack.Value = tier * 10;
                player.defense.Value = tier * 10;
                player.vigor.Value = tier * 10;

                player.maxHealth.Value = 100 + tier * 80;
            }


            return ChatData.Info("Set max tier: " + tier);
        }
    }
}
