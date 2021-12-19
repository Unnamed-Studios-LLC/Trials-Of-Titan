using ServerNode.Net.Packets;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using Utils.NET.Net.Tcp;

namespace ServerNode.Net
{
    public class ProgramConnection : NetConnection<NPacket>
    {
        public const int Port = 2434;

        public ProgramConnection(Socket socket) : base(socket) { }

        public ProgramConnection() : base() { }

        public ProgramConnection(bool verified) : base() { this.verified = verified; }

        private bool verified = false;

        public int programType = -1;

        public int processId = -1;

        public event Action<ProgramConnection> onVerify;

        public Dictionary<NPacketType, Action<NPacket, ProgramConnection>> handlers = new Dictionary<NPacketType, Action<NPacket, ProgramConnection>>();

        protected override void HandlePacket(NPacket packet)
        {
            if (!verified)
            {
                if (packet is NProgramVerify verify && verify.IsValid())
                {
                    verified = true;
                    programType = verify.programType;
                    processId = verify.processId;
                    onVerify?.Invoke(this);
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
