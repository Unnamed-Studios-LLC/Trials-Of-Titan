using System;
using Utils.NET.IO;

namespace TitanCore.Net.Packets.Client
{
    public class TnChat : TnPacket
    {
        public override TnPacketType Type => TnPacketType.Chat;

        /// <summary>
        /// The text to send as chat to the server
        /// </summary>
        public string text;

        public TnChat()
        {
        }

        public TnChat(string text)
        {
            if (text.Length > NetConstants.Max_Chat_Length)
                text = text.Substring(0, NetConstants.Max_Chat_Length);
            this.text = text;
        }

        protected override void Read(BitReader r)
        {
            text = r.ReadUTF(NetConstants.Max_Chat_Length);
        }

        protected override void Write(BitWriter w)
        {
            w.Write(text);
        }
    }
}
