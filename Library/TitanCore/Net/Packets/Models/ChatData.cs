using System;
using Utils.NET.IO;

namespace TitanCore.Net.Packets.Models
{
    public class ChatData
    {
        public enum ChatOwner
        {
            Error = -1,
            Info = -2,
            Mannah = -3
        }

        public static ChatData Error(string message)
        {
            return new ChatData((long)ChatOwner.Error, message);
        }

        public static ChatData Info(string message)
        {
            return new ChatData((long)ChatOwner.Info, message);
        }

        public static ChatData Mannah(string message)
        {
            return new ChatData((long)ChatOwner.Mannah, message);
        }

        public static ChatData ReadChatData(BitReader r)
        {
            var chatData = new ChatData();
            chatData.Read(r);
            return chatData;
        }

        /// <summary>
        /// The gameId of the owner
        /// </summary>
        public long ownerGameId;

        /// <summary>
        /// The text contained in the chat
        /// </summary>
        public string text;

        public ChatData()
        {
        }

        public ChatData(long ownerGameId, string text)
        {
            this.ownerGameId = ownerGameId;
            this.text = text;
        }

        public void Read(BitReader r)
        {
            ownerGameId = r.ReadInt64();
            text = r.ReadUTF(NetConstants.Max_Chat_Length);
        }

        public void Write(BitWriter w)
        {
            w.Write(ownerGameId);
            w.Write(text);
        }
    }
}
