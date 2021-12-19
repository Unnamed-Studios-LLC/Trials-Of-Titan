using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using TitanCore.Data;
using TitanCore.Net.Packets.Models;
using World.Map.Objects.Entities;
using World.Map.Objects.Map;

namespace World.Commands
{
    public class AscendCommand : CommandHandler
    {
        public override Rank MinRank => Rank.Admin;

        public override string Command => "ascend";

        public override string Syntax => "/ascend";

        public override ChatData Handle(Player player, CommandArgs args)
        {
            var info = GameData.objects[0xa93];
            var obj = new AscensionTable();
            obj.Initialize(info);
            obj.position.Value = player.position.Value;
            player.world.objects.AddObject(obj);

            return ChatData.Info("Success");
        }
    }
}
