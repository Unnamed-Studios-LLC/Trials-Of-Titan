using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using TitanCore.Net.Packets.Models;
using Utils.NET.Utils;
using World.Map.Objects.Entities;

namespace World.Commands
{
    public class EffectCommand : CommandHandler
    {
        public override Rank MinRank => Rank.Admin;

        public override string Command => "eff";

        public override string Syntax => "/eff {effect name or id} {effect2} {etc..}";

        public override ChatData Handle(Player player, CommandArgs args)
        {
            if (args.args.Length == 0)
                return SyntaxError;

            var effects = new HashSet<StatusEffect>();
            for (int i = 0; i < args.args.Length; i++)
            {
                if (!Enum.TryParse<StatusEffect>(args.args[i], out var effect))
                {
                    return ChatData.Error($"Unable to parse effect '{args.args[i]}'");
                }
                effects.Add(effect);
            }

            var removed = new List<StatusEffect>();
            var added = new List<StatusEffect>();
            foreach (var effect in effects)
            {
                if (player.HasServerEffect(effect))
                {
                    player.RemoveEffect(effect);
                    removed.Add(effect);
                }
                else
                {
                    player.AddEffect(effect, 100000);
                    added.Add(effect);
                }
            }

            var builder = new StringBuilder();
            if (added.Count > 0)
                builder.Append($"Added effect{(added.Count > 1 ? "s" : "")}: {StringUtils.ComponentsToString(',', added.ToArray())}");
            if (removed.Count > 0)
            {
                if (added.Count > 0)
                    builder.Append('\n');
                builder.Append($"Removed effect{(removed.Count > 1 ? "s" : "")}: {StringUtils.ComponentsToString(',', removed.ToArray())}");
            }
            return ChatData.Info(builder.ToString());
        }
    }
}
