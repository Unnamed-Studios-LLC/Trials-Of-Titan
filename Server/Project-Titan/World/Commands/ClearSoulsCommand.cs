using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using TitanCore.Net.Packets.Models;
using World.Map.Objects.Entities;

namespace World.Commands
{
    public class ClearSoulsCommand : CommandHandler
    {
        public override Rank MinRank => Rank.Admin;

        public override string Command => "clearsouls";

        public override string Syntax => "/clearsouls";

        public override ChatData Handle(Player player, CommandArgs args)
        {
            player.RemoveFullSouls((ulong)player.fullSouls.Value);
            return ChatData.Info("Successfully cleared souls");
        }
    }
}
