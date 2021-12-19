using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TitanCore.Core;
using TitanCore.Data;
using TitanCore.Data.Items;
using TitanCore.Net;
using TitanCore.Net.Packets.Models;
using TitanDatabase;
using Utils.NET.Utils;
using World.Map.Objects.Entities;

namespace World.Commands
{
    public class GiveCommand : CommandHandler
    {
        public override Rank MinRank => Rank.Admin;

        public override string Command => "give";

        public override string Syntax => "/give {item type or name}";

        public override ChatData Handle(Player player, CommandArgs args)
        {
            if (args.args.Length == 0) return SyntaxError;

            GameObjectInfo info;
            if (args.args.Length == 1 && args.args[0].StartsWith("0x"))
            {
                ushort type = (ushort)StringUtils.ParseHex(args.args[0]);
                GameData.objects.TryGetValue(type, out info);

                if (info == null)
                    return ChatData.Error("Unable to find item type: 0x" + type.ToString("X"));
            }
            else
            {
                var name = StringUtils.ComponentsToString(' ', args.args);
                info = GameData.GetObjectByName(name);

                if (info == null)
                {
                    var search = GameData.Search(name).Where(_ => _ is ItemInfo).ToArray();
                    if (search.Length != 1)
                    {
                        var builder = new StringBuilder();
                        builder.Append("Unable to find item: " + name);
                        if (search.Length <= 10)
                        {
                            if (search.Length > 1)
                                builder.Append("\nDid you mean:");
                            foreach (var obj in search)
                            {
                                builder.Append('\n');
                                builder.Append(obj.name);
                            }
                        }
                        if (builder.Length >= NetConstants.Max_Chat_Length)
                            return ChatData.Error("Results are too large");
                        return ChatData.Error(builder.ToString());
                    }
                    info = search[0];
                }
            }

            if (!(info is ItemInfo itemInfo))
                return ChatData.Error($"'{info.name}' is not an item!");

            player.StartItemAction();
            DoGiveItem(player, new Item(info.id));

            return null;
        }

        private async void DoGiveItem(Player player, Item item)
        {
            var createResponse = await Database.CreateItem(item, player.character.id);
            player.PushTickAction(obj =>
            {
                var pp = (Player)obj;
                ChatData chat;
                switch (createResponse.result)
                {
                    case CreateItemResult.Success:
                        if (!pp.TryGiveItem(createResponse.item))
                            chat = ChatData.Error($"Failed to give item. Your inventory is full!");
                        else
                            chat = ChatData.Info($"Successfully given '{createResponse.item.itemData.GetInfo().name}'");
                        break;
                    default:
                        chat = ChatData.Error($"Internal error, failed to create item");
                        break;
                }
                pp.AddChat(chat);
                pp.EndItemAction();
            });
        }
    }
}
