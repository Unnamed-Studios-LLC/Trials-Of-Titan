using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using TitanDatabase.Broadcasting.Packets;
using Utils.NET.Net.Tcp;

namespace TitanDatabase.Broadcasting
{
    public class BroadcastListenerConnection : NetConnection<BrPacket>
    {
        public BroadcastListener listener;

        public string serverName;

        private bool verified;

        public BroadcastListenerConnection(Socket socket) : base(socket) { }

        protected override void HandlePacket(BrPacket packet)
        {
            if (!verified)
            {
                if (packet.Type != BrPacketType.Verify) return;
                var verify = (BrVerify)packet;
                if (verify.IsValid())
                {
                    verified = true;
                    serverName = verify.serverName;
                    listener.Verify(this);
                }
                else
                {
                    listener.RemoveUnverifiedConnection(this);
                    Disconnect();
                }
                return;
            }

            listener.ReceivedPacket(packet, this);
        }

        protected override void OnDisconnect()
        {
            if (verified)
                listener.ConnectionDisconnected(this);
            else
                listener.RemoveUnverifiedConnection(this);
        }
    }
}
