using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;

namespace Utils.NET.Net.Udp.Packets
{
    public class UdpDisconnect : UdpPacket
    {
        public override UdpPacketType Type => UdpPacketType.Disconnect;

        /// <summary>
        /// Generated salt from server/client
        /// </summary>
        public ulong salt;

        /// <summary>
        /// The reason for disconnect
        /// </summary>
        public UdpDisconnectReason reason;

        /// <summary>
        /// Custom set disconenct reason
        /// </summary>
        public string message;

        /// <summary>
        /// Gets the disconnect reason string
        /// </summary>
        public string ReasonString => reason == UdpDisconnectReason.Custom ? message : reason.ToString();

        public UdpDisconnect() { }

        public UdpDisconnect(ulong salt, UdpDisconnectReason reason, string message)
        {
            this.salt = salt;
            this.reason = reason;
            this.message = message;
        }

        public UdpDisconnect(ulong salt, UdpDisconnectReason reason)
        {
            this.salt = salt;
            this.reason = reason;
        }

        public UdpDisconnect(ulong salt, string message)
        {
            this.salt = salt;
            reason = UdpDisconnectReason.Custom;
            this.message = message;
        }

        protected override void Read(BitReader r)
        {
            salt = r.ReadUInt64();
            reason = (UdpDisconnectReason)r.Read(4);
            if (reason == UdpDisconnectReason.Custom)
                message = r.ReadUTF(256);
        }

        protected override void Write(BitWriter w)
        {
            w.Write(salt);
            w.Write((byte)reason, 4);
            if (reason == UdpDisconnectReason.Custom)
                w.Write(message);
        }
    }
}
