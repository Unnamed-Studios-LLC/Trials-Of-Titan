using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils.NET.IO;
using Utils.NET.Logging;

namespace Utils.NET.Net.Udp.Reliability
{
    public class OrderedPacket<TPacket> : Packet where TPacket : Packet
    {
        public override string ToString() => packet.ToString();

        public override byte Id => packet.Id;

        /// <summary>
        /// The order id of this packet
        /// </summary>
        public uint orderId;

        /// <summary>
        /// The base packet to send
        /// </summary>
        public TPacket packet;

        protected override void Read(BitReader r)
        {
            orderId = r.ReadUInt32();
            packet.ReadPacket(r);
        }

        protected override void Write(BitWriter w)
        {
            w.Write(orderId);
            packet.WritePacket(w);
        }
    }

    public class OrderedReliableChannel<TPacket> : PacketChannel<TPacket> where TPacket : Packet
    {
        private class OrderedPacketFactory : PacketFactory<OrderedPacket<TPacket>>
        {
            protected override Dictionary<byte, Type> GetPacketTypes()
            {
                var t = typeof(TPacket);
                return t.Assembly.GetTypes().Where(_ => _.IsSubclassOf(t)).ToDictionary(_ => ((TPacket)Activator.CreateInstance(_)).Id);
            }

            public override OrderedPacket<TPacket> CreatePacket(byte id)
            {
                if (!packetTypes.TryGetValue(id, out var type))
                    return null;
                var packet = (TPacket)Activator.CreateInstance(type);
                var ordered = new OrderedPacket<TPacket>();
                ordered.packet = packet;
                return ordered;
            }
        }

        /// <summary>
        /// The next packet order id to send
        /// </summary>
        private uint nextSendOrderId = 0;

        /// <summary>
        /// The next packet order id to receive
        /// </summary>
        private uint nextReceiveOrderId = 0;

        /// <summary>
        /// Packets received early
        /// </summary>
        private Dictionary<uint, TPacket> receivedPackets = new Dictionary<uint, TPacket>();

        /// <summary>
        /// Underlying reliability channel
        /// </summary>
        private ReliableChannel<OrderedPacket<TPacket>> reliableChannel = new ReliableChannel<OrderedPacket<TPacket>>();

        public OrderedReliableChannel()
        {
            reliableChannel.SetFactory(new OrderedPacketFactory());
            reliableChannel.SetReceiveAction(ReceiveOrderedPacket);
        }

        public override void SetWriteUdpHeader(Action<BitWriter, UdpSendData> doWriteUdpHeader)
        {
            base.SetWriteUdpHeader(doWriteUdpHeader);
            reliableChannel.SetWriteUdpHeader(doWriteUdpHeader);
        }

        public override void SetSendAction(Action<IO.Buffer> doSendPacket)
        {
            base.SetSendAction(doSendPacket);
            reliableChannel.SetSendAction(doSendPacket);
        }

        public override void ReceivePacket(BitReader r, byte packetId)
        {
            reliableChannel.ReceivePacket(r, packetId);
        }

        private void ReceiveOrderedPacket(OrderedPacket<TPacket> ordered)
        {
            if (ordered.orderId < nextReceiveOrderId) return; // already received
            if (ordered.orderId == nextReceiveOrderId)
            {
                nextReceiveOrderId++;
                doReceivePacket(ordered.packet);

                while (receivedPackets.TryGetValue(nextReceiveOrderId, out var packet))
                {
                    receivedPackets.Remove(nextReceiveOrderId);
                    doReceivePacket(packet);
                    nextReceiveOrderId++;
                }
            }
            else
            {
                receivedPackets[ordered.orderId] = ordered.packet;
            }
        }

        public override void SendPacket(BitWriter w, TPacket packet)
        {
            var ordered = new OrderedPacket<TPacket>();
            ordered.orderId = nextSendOrderId++;
            ordered.packet = packet;
            reliableChannel.SendPacket(w, ordered);
        }
    }
}
