using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Utils.NET.Modules;
using Utils.NET.Net.Tcp;
using World.Instances.Packets;

namespace World.Instances
{
    public class InstanceToManagerConnection : NetConnection<InPacket>
    {
        public WorldModule module;

        public InstanceToManagerConnection() : base()
        {
        }

        protected override void HandlePacket(InPacket packet)
        {
            switch (packet.Type)
            {
                case InPacketType.WorldKeyRequest:
                    HandleWorldKeyRequest((InWorldKeyRequest)packet);
                    break;
            }
        }

        private void HandleWorldKeyRequest(InWorldKeyRequest request)
        {
            var response = new InWorldKeyResult();
            response.Token = request.Token;
            if (module == null || module.worldManager == null)
            {
                response.key = 0;
                SendTokenResponseAsync(response);
                return;
            }

            if (!module.worldManager.TryGetWorld(request.worldId, out var world))
            {
                response.key = 0;
            }
            else
            {
                if (!world.TryGenerateKey(request.accountId, out response.key))
                {
                    response.key = 0;
                }
            }
            SendTokenResponseAsync(response);
        }

        protected override void OnDisconnect()
        {
            if (module.closed) return;
            ModularProgram.Exit();
        }

        public void SendPlayerCount(int count)
        {
            SendAsync(new InPlayerCount(count));
        }

        public void RequestWorldKey(ulong accountId, uint worldId, Action<ulong> callback)
        {
            SendTokenAsync(new InWorldKeyRequest(accountId, worldId), (packet, c) =>
            {
                var result = (InWorldKeyResult)packet;
                callback?.Invoke(result.key);
            });
        }
    }
}
