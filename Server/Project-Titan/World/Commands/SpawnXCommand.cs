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
    public class SpawnXCommand : CommandHandler
    {
        public override Rank MinRank => Rank.Admin;

        public override string Command => "spawnx";

        public override string Syntax => "/spawnx {amount} {enemy type or name}";

        public override ChatData Handle(Player player, CommandArgs args)
        {
            GameObjectInfo info;
            if (args.args.Length < 2 || !uint.TryParse(args.args[0], out var count))
                return SyntaxError;

            if (args.args.Length == 2 && args.args[0].StartsWith("0x"))
            {
                ushort type = (ushort)StringUtils.ParseHex(args.args[0]);
                GameData.objects.TryGetValue(type, out info);

                if (info == null)
                    return ChatData.Error("Unable to find enemy type: 0x" + type.ToString("X"));
            }
            else
            {
                string[] nameArgs = new string[args.args.Length - 1];
                Array.Copy(args.args, 1, nameArgs, 0, nameArgs.Length);
                var name = StringUtils.ComponentsToString(' ', nameArgs);
                info = GameData.GetObjectByName(name);

                if (info == null)
                    return ChatData.Error("Unable to find enemy: " + name);
            }

            for (int i = 0; i < count; i++)
            {
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
            }

            return ChatData.Info($"Successfully spawned '{info.name}'");
        }
    }
}
