using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;
using Utils.NET.Logging;

namespace Utils.NET.Net.Udp.Reliability
{
    public class ReliableChannel<TPacket> : PacketChannel<TPacket> where TPacket : Packet
    {
        protected struct PacketHeader
        {
            /// <summary>
            /// The sequence id of this packet
            /// </summary>
            public ushort sequenceId;

            /// <summary>
            /// The last received sequence id
            /// </summary>
            public ushort lastReceivedId;

            /// <summary>
            /// Bitfield containing received sequence info
            /// </summary>
            public uint receivedBitfield;

            public PacketHeader(BitReader r)
            {
                sequenceId = r.ReadUInt16();
                lastReceivedId = r.ReadUInt16();
                receivedBitfield = r.ReadUInt32();
            }
        }

        /// <summary>
        /// Dictionary containing all sent packets awaiting acknowledgement
        /// </summary>
        protected ConcurrentDictionary<ushort, TPacket> sentPackets = new ConcurrentDictionary<ushort, TPacket>();

        /// <summary>
        /// The next packet sequence id
        /// </summary>
        private ushort nextSequenceId = 0;

        /// <summary>
        /// The last received sequence id
        /// </summary>
        private ushort lastReceivedSequenceId = 0;

        /// <summary>
        /// Bitfield representing received packets
        /// </summary>
        private uint receivedBitfield = 0;

        /// <summary>
        /// Object used to sync sequence work
        /// </summary>
        private object sequenceLock = new object();

        public override void ReceivePacket(BitReader r, byte packetId)
        {
            var header = ReadHeader(r); // process header
            if (!HandlePacketHeader(header)) return;

            TPacket packet = packetFactory.CreatePacket(packetId);
            if (packet == null)
            {
                Log.Error($"No {typeof(TPacket).Name} for id: {packetId}");
                return;
            }
            packet.ReadPacket(r);
            HandlePacket(packet);
        }

        /// <summary>
        /// Handles a received packet
        /// </summary>
        /// <param name="packet"></param>
        protected void HandlePacket(TPacket packet)
        {
            doReceivePacket(packet);
        }

        /// <summary>
        /// Reads a packet header from a given BitReader
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        protected PacketHeader ReadHeader(BitReader r)
        {
            return new PacketHeader(r);
        }

        /// <summary>
        /// Handles a received packet header
        /// </summary>
        /// <param name="header"></param>
        protected bool HandlePacketHeader(PacketHeader header)
        {
            //Log.Write("Received seq id: " + header.sequenceId);
            HandleReceivedBitfield(header.lastReceivedId, header.receivedBitfield);
            return HandleSequenceId(header.sequenceId);
        }

        /// <summary>
        /// Handles remote's sequence id and logs the receive info in the local bitfield
        /// </summary>
        /// <param name="sequenceId"></param>
        /// <returns></returns>
        private bool HandleSequenceId(ushort sequenceId)
        {
            lock (sequenceLock)
            {
                var currentId = lastReceivedSequenceId;
                var currentBitfield = receivedBitfield;
                if (lastReceivedSequenceId == sequenceId) return false; // already processed this

                var dif = GetSequenceDifference(currentId, sequenceId);

                if (dif > 0) // advance sequence packet
                {
                    lastReceivedSequenceId = sequenceId;
                    if (dif == 32)
                    {
                        receivedBitfield = (uint)1 << 31;
                    }
                    else if (dif <= 32)
                    {
                        receivedBitfield = (receivedBitfield << dif) | ((uint)1 << (dif - 1));
                    }
                    else
                    {
                        receivedBitfield = 0;
                    }

                    return true;
                }
                else // don't advance sequence
                {
                    dif = -dif;
                    if (dif <= 32)
                    {
                        bool alreadyProcessed = ((receivedBitfield >> (dif - 1)) & 1) == 1;
                        if (alreadyProcessed) return false; // already received / processed
                        receivedBitfield |= (uint)1 << (dif - 1);
                        return true;
                    }
                    return false;
                }
            }
        }

        /// <summary>
        /// Handles the header information regarding remote received packets
        /// </summary>
        /// <param name="lastReceivedId"></param>
        /// <param name="bitfield"></param>
        private void HandleReceivedBitfield(ushort lastReceivedId, uint bitfield)
        {
            TPacket dummy;
            if (sentPackets.TryRemove(lastReceivedId, out dummy))
            {
                //Log.Write("Packet confirmed: " + lastReceivedId);
            }
            foreach (var packet in sentPackets.ToArray())
            {
                var dif = GetSequenceDifference(packet.Key, lastReceivedId);
                if (dif > 32) // packet lost
                {
                    sentPackets.TryRemove(packet.Key, out dummy);
                    ResendPacket(packet.Value); // TODO implement congestion control
                } 
                else if (dif > 0 && ((bitfield >> (dif - 1)) & 1) == 1) // packet received successfully
                {
                    sentPackets.TryRemove(packet.Key, out dummy);
                    //Log.Write("Packet confirmed: " + packet.Key);
                }
            }
        }

        /// <summary>
        /// Returns the difference between two sequence id's
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        private int GetSequenceDifference(ushort from, ushort to)
        {
            if (to == from) return 0;

            int highDif;
            if (to < from)
                highDif = to + (ushort.MaxValue - from + 1);
            else
                highDif = to - from;

            if (highDif > ushort.MaxValue / 2)
            {
                int lowDif;
                if (from < to)
                    lowDif = from + (ushort.MaxValue - to + 1);
                else
                    lowDif = from - from;
                return -lowDif;
            }
            else
                return highDif;
        }

        /// <summary>
        /// Resends a given packet
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="originalSequenceId"></param>
        private void ResendPacket(TPacket packet)
        {
            var w = new BitWriter();
            doWriteUdpHeader(w, new UdpSendData(packet, false));
            SendPacket(w, packet);
        }

        public override void SendPacket(BitWriter w, TPacket packet)
        {
            ushort seq = WriteHeader(w);
            packet.WritePacket(w);
            sentPackets[seq] = packet;
            doSendPacket(w.GetData());
        }

        /// <summary>
        /// Writes the Udp packet header
        /// </summary>
        protected ushort WriteHeader(BitWriter w)
        {
            ushort seq = ++nextSequenceId;
            w.Write(seq);
            GetReceivedInfo(out ushort latest, out uint bitfield);
            w.Write(latest);
            w.Write(bitfield);
            return seq;
        }

        /// <summary>
        /// Gets the received information atmoically
        /// </summary>
        /// <param name="latest"></param>
        /// <param name="bitfield"></param>
        protected void GetReceivedInfo(out ushort latest, out uint bitfield)
        {
            lock (sequenceLock)
            {
                latest = lastReceivedSequenceId;
                bitfield = receivedBitfield;
            }
        }
    }
}
