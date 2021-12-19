using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using TitanCore.Net.Packets.Models;
using World.Map.Objects.Entities;

namespace World.Commands
{
    public class ClearInvCommand : CommandHandler
    {
        public override Rank MinRank => Rank.Admin;

        public override string Command => "clearinv";

        public override string Syntax => "/clearinv {startIndex} {length}";

        public override ChatData Handle(Player player, CommandArgs args)
        {
            if (args.args.Length != 2) return SyntaxError;

            int from, length;
            if (!int.TryParse(args.args[0], out from))
                return ChatData.Error("Invalid start index");
            if (!int.TryParse(args.args[0], out length))
                return ChatData.Error("Invalid length");

            for (int i = from; i < from + length; i++)
            {
                player.SetItem(i, null);
            }
            return ChatData.Info($"Successfully cleared slot {from} to slot {from + length}");
        }
    }
}
