using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using TitanCore.Net.Packets.Models;
using World.Map.Objects.Entities;

namespace World.Commands
{
    public class LevelCommand : CommandHandler
    {
        public override Rank MinRank => Rank.Admin;

        public override string Command => "level";

        public override string Syntax => "/level";

        public override ChatData Handle(Player player, CommandArgs args)
        {
            player.LevelUp();
            return ChatData.Info("Success");
        }
    }
}
