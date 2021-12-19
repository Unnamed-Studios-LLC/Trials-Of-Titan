using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using TitanCore.Net.Packets.Models;
using World.Map.Objects.Entities;

namespace World.Commands
{
    public abstract class CommandHandler
    {
        public abstract Rank MinRank { get; }

        public abstract string Command { get; }

        public abstract string Syntax { get; }

        protected ChatData SyntaxError => ChatData.Error("Invalid input, correct syntax: " + Syntax);

        public abstract ChatData Handle(Player player, CommandArgs args);
    }
}
