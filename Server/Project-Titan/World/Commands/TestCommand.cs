using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TitanCore.Core;
using TitanCore.Data;
using TitanCore.Data.Entities;
using TitanCore.Data.Items;
using TitanCore.Net.Packets.Models;
using TitanDatabase;
using TitanDatabase.Models;
using World.Map.Objects.Entities;

namespace World.Commands
{
    public class TestCommand : CommandHandler
    {
        public override Rank MinRank => Rank.Admin;

        public override string Command => "test";

        public override string Syntax => "/test";

        public override ChatData Handle(Player player, CommandArgs args)
        {
            if (player.GetStatisticValue(CharacterStatisticType.SoulsEarned) != 0)
            {
                return ChatData.Error("Must be a new character");
            }

            player.AddFullSouls(30000);

            return ChatData.Info("Success");
        }
    }
}
