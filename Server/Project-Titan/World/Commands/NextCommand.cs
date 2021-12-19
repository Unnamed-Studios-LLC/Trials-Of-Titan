using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using TitanCore.Net.Packets.Models;
using World.Map.Objects.Entities;
using World.Worlds;

namespace World.Commands
{
    public class NextCommand : CommandHandler
    {
        public override Rank MinRank => Rank.Admin;

        public override string Command => "next";

        public override string Syntax => "/next";

        public override ChatData Handle(Player player, CommandArgs args)
        {
            if (!(player.world is Overworld overworld))
                return ChatData.Error("'Next' command is only usable in the Overworld");

            //overworld.titanSpawnSystem.Next();
            return ChatData.Info("Success");
        }
    }
}
