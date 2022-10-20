using System;
using Utils.NET.Net.Udp.Packets;
using Utils.NET.Utils;

namespace Utils.NET.Net.Udp
{
    public class UdpSendData
    {
        public Packet packet;
        public bool udp;

        public UdpSendData(Packet packet, bool udp)
        {
            this.packet = packet;
            this.udp = udp;
        }
    }

    public static class Udp
    {
        /// <summary>
        /// Generates a salt value
        /// </summary>
        /// <returns>The local salt.</returns>
        public static ulong GenerateLocalSalt()
        {
            ulong a = (ulong)Rand.IntValue();
            ulong b = (ulong)Rand.IntValue();
            return (a << 32) | b;
        }

        /// <summary>
        /// Creates salt from client and server generated values
        /// </summary>
        /// <returns>The salt.</returns>
        /// <param name="client">Client.</param>
        /// <param name="server">Server.</param>
        public static ulong CreateSalt(ulong client, ulong server)
        {
            return client ^ server;
        }

        /// <summary>
        /// Creates a UdpPacket from a given type id
        /// </summary>
        /// <returns>The UDP packet.</returns>
        /// <param name="id">Identifier.</param>
        public static UdpPacket CreateUdpPacket(byte id)
        {
            switch ((UdpPacketType)id)
            {
                case UdpPacketType.Connect:
                    return new UdpConnect();
                case UdpPacketType.Challenge:
                    return new UdpChallenge();
                case UdpPacketType.Solution:
                    return new UdpSolution();
                case UdpPacketType.Connected:
                    return new UdpConnected();
                case UdpPacketType.Disconnect:
                    return new UdpDisconnect();
            }
            return null;
        }

    }
}
