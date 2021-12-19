using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using TitanCore.Net.Packets.Models;
using World.Map.Objects.Entities;
using World.Worlds;

namespace World.Commands
{
    public class MannahCommand : CommandHandler
    {
        public override Rank MinRank => Rank.Admin;

        public override string Command => "mannah";

        public override string Syntax => "/mannah";

        public override ChatData Handle(Player player, CommandArgs args)
        {
            if (!(player.world is Overworld overworld))
                return ChatData.Error("This command is only available in the overworld!");

            //overworld.titanSpawnSystem.StartMannah();
            return ChatData.Info("Success");
        }
    }
}
