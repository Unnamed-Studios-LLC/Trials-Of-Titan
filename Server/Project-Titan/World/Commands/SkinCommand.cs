using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TitanCore.Core;
using TitanCore.Data;
using TitanCore.Net.Packets.Models;
using Utils.NET.Utils;
using World.Map.Objects.Entities;

namespace World.Commands
{
    public class SkinCommand : CommandHandler
    {
        public override Rank MinRank => Rank.Admin;

        public override string Command => "skin";

        public override string Syntax => "/skin {object id or object name}";

        public override ChatData Handle(Player player, CommandArgs args)
        {
            GameObjectInfo info = null;
            if (args.args.Length == 1 && args.args[0].StartsWith("0x"))
            {
                ushort type = (ushort)StringUtils.ParseHex(args.args[0]);
                GameData.objects.TryGetValue(type, out info);

                if (info == null)
                    return ChatData.Error("Unable to find object type: 0x" + type.ToString("X"));
            }
            else
            {
                var name = StringUtils.ComponentsToString(' ', args.args);
                var results = GameData.Search(name).ToArray();

                if (results.Length == 0)
                {
                    return ChatData.Error("Unable to find object named: " + name);
                }
                else if (results.Length == 1)
                {
                    info = results[0];
                }
                else
                {
                    foreach (var result in results)
                    {
                        if (result.name.Equals(name, StringComparison.OrdinalIgnoreCase))
                        {
                            info = result;
                            break;
                        }
                    }

                    if (info == null)
                    {
                        var msg = "Possible results: ";
                        for (int i = 0; i < results.Length; i++)
                        {
                            if (i != 0)
                                msg += ", ";
                            msg += results[i].name;
                        }
                        return ChatData.Info(msg);
                    }
                }
            }

            player.skin.Value = info.id;
            return ChatData.Info("Success");
        }
    }
}
