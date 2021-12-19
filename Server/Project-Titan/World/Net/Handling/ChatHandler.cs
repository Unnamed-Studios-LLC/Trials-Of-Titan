using System;
using TitanCore.Net.Packets.Client;
using TitanCore.Net.Packets.Models;

namespace World.Net.Handling
{
    public class ChatHandler : ClientPacketHandler<TnChat>
    {
        public override void Handle(TnChat packet, Client connection)
        {
            if (!connection.chatSpamRegulator.Event())
            {
                connection.player.AddChat(ChatData.Error("You are chatting too fast!"));
                return;
            }

            var text = packet.text;
            if (text.StartsWith("/", StringComparison.Ordinal)) // the text is a command
            {
                var args = connection.player.CreateCommand(text);
                if (args == null) return;
                connection.player.ProcessCommand(args);
            }
            else
            {
                if (connection.account.mutedUntil > DateTime.UtcNow) return;
                var chat = new ChatData(connection.player.gameId, text);
                foreach (var player in connection.player.playersSentTo)
                    player.AddChat(chat);
            }
        }
    }
}
