using ServerNode.Net.Packets;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using Utils.NET.Logging;
using Utils.NET.Net;
using Utils.NET.Net.Tcp;

namespace ServerNode.Net
{
    public class NodeConnection : NetConnection<NPacket>
    {
        public const int Port = 3476;

        public NodeConnection(Socket socket) : base(socket) { }

        public NodeConnection() : base() { }

        public NodeConnection(bool verified) : base() { this.verified = verified; }

        private bool verified = false;

        public Dictionary<NPacketType, Action<NPacket, NodeConnection>> handlers = new Dictionary<NPacketType, Action<NPacket, NodeConnection>>();

        protected override void HandlePacket(NPacket packet)
        {
            if (!verified)
            {
                if (packet is NNodeVerify verify && verify.IsValid())
                {
                    Log.Write("Verified connection");
                    verified = true;
                }
                else Disconnect();
                return;
            }

            if (!handlers.TryGetValue(packet.Type, out var handler)) return;
            handler?.Invoke(packet, this);
        }

        protected override void OnDisconnect()
        {
        }
    }
}
