using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using TitanCore.Net.Packets.Models;
using World.Map.Objects.Entities;

namespace World.Commands
{
    public class SetStatCommand : CommandHandler
    {
        public override Rank MinRank => Rank.Admin;

        public override string Command => "setstat";

        public override string Syntax => "/setstat {stat} {value}";

        public override ChatData Handle(Player player, CommandArgs args)
        {
            if (args.args.Length != 2) return SyntaxError;

            if (!Enum.TryParse<StatType>(args.args[0], true, out var type))
                return ChatData.Error("Unable to parse given stat type");

            if (!int.TryParse(args.args[1], out var value))
                return ChatData.Error("Unable to parse given stat value");

            player.SetStatBase(type, value);
            return ChatData.Info($"Successfully set {type} to {value}");
        }
    }
}
