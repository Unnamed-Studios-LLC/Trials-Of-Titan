using System;
using TitanCore.Net.Packets.Models;
using Utils.NET.IO;

namespace TitanCore.Net.Packets.Server
{
    public class TnChats : TnPacket
    {
        public override TnPacketType Type => TnPacketType.Chats;

        public ChatData[] chats;

        public TnChats()
        {
        }

        public TnChats(ChatData[] chats)
        {
            this.chats = chats;
        }

        protected override void Read(BitReader r)
        {
            chats = new ChatData[r.ReadUInt32()];
            for (int i = 0; i < chats.Length; i++)
                chats[i] = ChatData.ReadChatData(r);
        }

        protected override void Write(BitWriter w)
        {
            w.Write(chats.Length);
            for (int i = 0; i < chats.Length; i++)
                chats[i].Write(w);
        }
    }
}
