using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;

namespace Utils.NET.Net.Udp.Reliability
{
    public abstract class PacketChannel<TPacket> where TPacket : Packet
    {
        protected PacketFactory<TPacket> packetFactory;

        protected Action<BitWriter, UdpSendData> doWriteUdpHeader;

        protected Action<IO.Buffer> doSendPacket;

        protected Action<TPacket> doReceivePacket;

        /// <summary>
        /// Sets the packet factory of this channel
        /// </summary>
        /// <param name="packetFactory"></param>
        public virtual void SetFactory(PacketFactory<TPacket> packetFactory)
        {
            this.packetFactory = packetFactory;
        }

        /// <summary>
        /// Sets the action used to handle received packets
        /// </summary>
        /// <param name="doReceivePacket"></param>
        public virtual void SetReceiveAction(Action<TPacket> doReceivePacket)
        {
            this.doReceivePacket = doReceivePacket;
        }

        /// <summary>
        /// Sets the action used to send packets
        /// </summary>
        /// <param name="doWriteUdpHeader"></param>
        public virtual void SetWriteUdpHeader(Action<BitWriter, UdpSendData> doWriteUdpHeader)
        {
            this.doWriteUdpHeader = doWriteUdpHeader;
        }

        /// <summary>
        /// Sets the action used to send packets
        /// </summary>
        /// <param name="doSendPacket"></param>
        public virtual void SetSendAction(Action<IO.Buffer> doSendPacket)
        {
            this.doSendPacket = doSendPacket;
        }

        public abstract void SendPacket(BitWriter w, TPacket packet);

        public abstract void ReceivePacket(BitReader r, byte packetId);
    }
}
