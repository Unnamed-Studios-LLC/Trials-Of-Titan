﻿using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;

namespace Utils.NET.Net.Udp.Packets
{
    public class UdpChallenge : UdpPacket
    {
        public override UdpPacketType Type => UdpPacketType.Challenge;

        /// <summary>
        /// Salt received from the client
        /// </summary>
        public ulong clientSalt;

        /// <summary>
        /// Salt generated by the server
        /// </summary>
        public ulong serverSalt;

        public UdpChallenge() { }

        public UdpChallenge(ulong clientSalt, ulong serverSalt)
        {
            this.clientSalt = clientSalt;
            this.serverSalt = serverSalt;
        }

        protected override void Read(BitReader r)
        {
            clientSalt = r.ReadUInt64();
            serverSalt = r.ReadUInt64();
        }

        protected override void Write(BitWriter w)
        {
            w.Write(clientSalt);
            w.Write(serverSalt);
        }
    }
}
