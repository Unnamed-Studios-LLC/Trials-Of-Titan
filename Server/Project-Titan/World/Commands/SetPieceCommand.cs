using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TitanCore.Core;
using TitanCore.Net.Packets.Models;
using World.Map;
using World.Map.Objects.Entities;

namespace World.Commands
{
    public class SetPieceCommand : CommandHandler
    {
        public override Rank MinRank => Rank.Admin;

        public override string Command => "setpiece";

        public override string Syntax => "/setpiece {name}";

        public override ChatData Handle(Player player, CommandArgs args)
        {
            if (args.args.Length != 1)
                return SyntaxError;

            var name = args.args[0];
            if (!File.Exists("Map/Files/SetPieces/" + name))
            {
                name += ".mef";
                if (!File.Exists("Map/Files/SetPieces/" + name))
                {
                    return ChatData.Error("Unabled to find set piece: " + args.args[0]);
                }
            }

            var setPiece = SetPiece.Load(name);
            var position = player.position.Value.ToInt2() + 1;
            player.world.ApplySetPiece(setPiece, position);
            for (int y = 0; y < setPiece.file.height; y++)
                for (int x = 0; x < setPiece.file.width; x++)
                {
                    foreach (var worldPlayer in player.world.objects.players.Values)
                    {
                        worldPlayer.gameState.SetDiscoveredTile(position.x + x, position.y + y, false);
                    }
                }

            return ChatData.Info("Successfully applied set piece: " + name);
        }
    }
}
