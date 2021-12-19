using System;
using System.Collections.Generic;
using System.Text;
using TitanDatabase.Broadcasting.Packets;
using Utils.NET.Net.Tcp;

namespace TitanDatabase.Broadcasting
{
    public class BroadcastConnection : NetConnection<BrPacket>
    {
        public Action<BrPacket> packetHandler;

        public BroadcastConnection() : base()
        {
        }

        protected override void HandlePacket(BrPacket packet)
        {
            packetHandler?.Invoke(packet);
        }

        protected override void OnDisconnect()
        {

        }
    }
}
