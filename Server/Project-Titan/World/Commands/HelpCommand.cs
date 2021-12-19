using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using TitanCore.Net.Packets.Models;
using World.Map.Objects.Entities;

namespace World.Commands
{
    public class HelpCommand : CommandHandler
    {
        public override Rank MinRank => Rank.Player;

        public override string Command => "help";

        public override string Syntax => "/help {optional command name}";

        public override ChatData Handle(Player player, CommandArgs args)
        {
            if (args.args.Length == 0)
            {
                return AvailableCommands(player);
            }
            else if (!CommandHandlerFactory.TryGetHandler(args.args[0], out var handler))
            {
                return ChatData.Error($"Unable to find command {args.args[0]}, try /help for a list of commands");
            }
            else
                return ChatData.Info($"Syntax: {handler.Syntax}");
        }

        private ChatData AvailableCommands(Player player)
        {
            var text = new StringBuilder("Available commands:");
            bool first = true;
            foreach (var handler in CommandHandlerFactory.AllHandlers)
                if (player.rank.Value >= (int)handler.MinRank)
                {
                    text.Append((first ? " " : ", ") + handler.Command);
                    first = false;
                }
            return ChatData.Info(text.ToString());
        }
    }
}
