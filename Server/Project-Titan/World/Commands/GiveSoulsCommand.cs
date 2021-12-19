using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using TitanCore.Net.Packets.Models;
using World.Map.Objects.Entities;

namespace World.Commands
{
    public class GiveSoulsCommand : CommandHandler
    {
        public override Rank MinRank => Rank.Admin;

        public override string Command => "givesouls";

        public override string Syntax => "/givesouls {amount}";

        public override ChatData Handle(Player player, CommandArgs args)
        {
            if (args.args.Length == 0)
                return SyntaxError;

            ulong amount;
            if (!ulong.TryParse(args.args[0], out amount))
            {
                return ChatData.Error("Failed to parse amount parameter, use syntax: " + Syntax);
            }

            player.AddFullSouls(amount);

            return ChatData.Info($"Successfully gave {amount} souls");
        }
    }
}
