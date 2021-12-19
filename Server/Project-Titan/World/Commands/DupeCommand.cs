using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using TitanCore.Net.Packets.Models;
using World.Map.Objects.Entities;

namespace World.Commands
{
    public class DupeCommand : CommandHandler
    {
        public override Rank MinRank => Rank.Admin;

        public override string Command => "dupe";

        public override string Syntax => "/dupe {slotId}";

        public override ChatData Handle(Player player, CommandArgs args)
        {
            if (args.args.Length == 0)
                return SyntaxError;

            if (!int.TryParse(args.args[0], out var slot))
                return ChatData.Error("Failed to parse the given slot");

            var item = player.GetItem(slot);
            if (item == null)
                return ChatData.Error("Given slot is empty");

            if (!player.TryGiveItem(item))
                ChatData.Error("Failed to give item, inventory is full");

            return ChatData.Info("Succesfully duped :)");
        }
    }
}
