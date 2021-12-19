using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Data;
using TitanCore.Net.Packets.Client;
using TitanCore.Net.Packets.Models;
using Utils.NET.Utils;

namespace World.Net.Handling
{
    public class EmoteHandler : ClientPacketHandler<TnEmote>
    {
        public override void Handle(TnEmote packet, Client connection)
        {
            if (connection.account == null) return;

            if (!connection.emoteSpamRegulator.Event())
            {
                connection.player.AddChat(ChatData.Error("You are emoting too much!"));
                return;
            }

            var emoteInfo = GameData.GetEmoteInfo(packet.emoteType);
            if (emoteInfo == null) return;

            if (!connection.account.HasUnlockedItem(emoteInfo.id))
            {
                connection.player.AddChat(ChatData.Error($"You have not unlocked the '{StringUtils.Labelize(packet.emoteType.ToString())}' emote!"));
                return;
            }
            connection.player.emote.Value = packet.emoteType;
        }
    }
}
