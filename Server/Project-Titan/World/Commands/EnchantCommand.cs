using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using TitanCore.Net.Packets.Models;
using Utils.NET.Utils;
using World.Map.Objects.Entities;

namespace World.Commands
{
    public class EnchantCommand : CommandHandler
    {
        public override Rank MinRank => Rank.Admin;

        public override string Command => "enchant";

        public override string Syntax => "/enchant {slot} {type} {level}";

        public override ChatData Handle(Player player, CommandArgs args)
        {
            if (args.args.Length != 3) return SyntaxError;

            if (!int.TryParse(args.args[0], out var slot))
                return ChatData.Error("Failed to parse the given slot");

            if (!Enum.TryParse<ItemEnchantType>(args.args[1], out var type))
                return ChatData.Error("Failed to parse the given enchantment type");

            if (!int.TryParse(args.args[2], out var level))
                return ChatData.Error("Failed to parse the given enchantment level");

            var serverItem = player.GetItem(slot);
            if (serverItem == null)
                return ChatData.Error($"Failed to apply enchantment: Slot {slot} is empty");
            var item = serverItem.itemData;

            item.enchantType = type;
            item.enchantLevel = (byte)level;
            serverItem.itemData = item;

            player.SetItem(slot, serverItem);

            return ChatData.Info($"Successfully enchanted {item.GetInfo().name} with {type.ToString()} {StringUtils.ToRoman(level)}");
        }
    }
}
