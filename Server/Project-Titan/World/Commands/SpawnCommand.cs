using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using TitanCore.Data;
using TitanCore.Net.Packets.Models;
using Utils.NET.Utils;
using World.Map.Objects.Entities;

namespace World.Commands
{
    public class SpawnCommand : CommandHandler
    {
        public override Rank MinRank => Rank.Admin;

        public override string Command => "spawn";

        public override string Syntax => "/spawn {enemy type or name}";

        public override ChatData Handle(Player player, CommandArgs args)
        {
            if (args.args.Length == 0) return SyntaxError;

            GameObjectInfo info;
            if (args.args.Length == 1 && args.args[0].StartsWith("0x"))
            {
                ushort type = (ushort)StringUtils.ParseHex(args.args[0]);
                GameData.objects.TryGetValue(type, out info);

                if (info == null)
                    return ChatData.Error("Unable to find enemy type: 0x" + type.ToString("X"));
            }
            else
            {
                var name = StringUtils.ComponentsToString(' ', args.args);
                info = GameData.GetObjectByName(name);

                if (info == null)
                    return ChatData.Error("Unable to find enemy: " + name);
            }

            if (info.Type == GameObjectType.Npc)
            {
                var npc = new Npc();
                npc.Initialize(info);
                npc.position.Value = player.position.Value;
                player.world.objects.AddObject(npc);
            }
            else
            {
                var enemy = player.world.objects.CreateEnemy(info);
                if (enemy == null)
                    return ChatData.Error(info.name + " is not an enemy type");
                enemy.position.Value = player.position.Value;
                player.world.objects.AddObject(enemy);
            }

            return ChatData.Info($"Successfully spawned '{info.name}'");
        }
    }
}
