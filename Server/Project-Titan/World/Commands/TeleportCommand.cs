using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using TitanCore.Net.Packets.Models;
using World.Map.Objects.Entities;

namespace World.Commands
{
    public class TeleportCommand : CommandHandler
    {
        public override Rank MinRank => Rank.Player;

        public override string Command => "teleport";

        public override string Syntax => "/teleport {player name}";

        public override ChatData Handle(Player player, CommandArgs args)
        {
            if (args.args.Length != 1)
                return SyntaxError;

            if (!player.world.objects.TryGetPlayer(args.args[0], out var otherPlayer))
            {
                return ChatData.Error($"Unable to find player: {args.args[0]}");
            }

            if (!player.world.AllowPlayerTeleport)
                return ChatData.Error($"This world does not allow teleporting");

            if (!player.Teleport(otherPlayer))
                return ChatData.Error($"Unable to teleport to specified player");

            return ChatData.Info("Successfully teleported to: " + otherPlayer.playerName.Value);
        }
    }

    public class TpCommand : CommandHandler
    {
        public override Rank MinRank => Rank.Player;

        public override string Command => "tp";

        public override string Syntax => "/tp {player name}";

        public override ChatData Handle(Player player, CommandArgs args)
        {
            if (args.args.Length != 1)
                return SyntaxError;

            if (!player.world.objects.TryGetPlayer(args.args[0], out var otherPlayer))
            {
                return ChatData.Error($"Unable to find player: {args.args[0]}");
            }

            if (!player.world.AllowPlayerTeleport)
                return ChatData.Error($"This world does not allow teleporting");

            if (!player.Teleport(otherPlayer))
                return ChatData.Error($"Unable to teleport to specified player");

            return ChatData.Info("Successfully teleported to: " + otherPlayer.playerName.Value);
        }
    }

    public class TpObjCommand : CommandHandler
    {
        public override Rank MinRank => Rank.Player;

        public override string Command => "tpobj";

        public override string Syntax => "/tpobj {object id}";

        public override ChatData Handle(Player player, CommandArgs args)
        {
            if (args.args.Length != 1)
                return SyntaxError;

            if (!uint.TryParse(args.args[0], out var gameId))
                return SyntaxError;

            if (!player.world.objects.TryGetObject(gameId, out var gameObject))
            {
                return ChatData.Error($"Unable to find specified object");
            }

            if (!player.Teleport(gameObject))
                return ChatData.Error($"Unable to teleport to specified object");

            return ChatData.Info("Successfully teleported to: " + gameObject.info.name);
        }
    }
}
