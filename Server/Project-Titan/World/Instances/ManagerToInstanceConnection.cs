using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using TitanCore.Net;
using Utils.NET.Logging;
using Utils.NET.Net.Tcp;
using World.Instances.Packets;
using World.Map.Objects.Map;

namespace World.Instances
{
    public class ManagerToInstanceConnection : NetConnection<InPacket>
    {
        public InstanceManager manager;

        public string instanceId;

        public string portalName;

        private bool verified = false;

        public Portal overworldPortal;

        public ManagerToInstanceConnection(Socket socket) : base(socket)
        {

        }

        protected override void HandlePacket(InPacket packet)
        {
            if (!verified)
            {
                if (packet.Type != InPacketType.Verify) return;
                var verify = (InVerify)packet;
                if (verify.IsValid())
                {
                    verified = true;
                    instanceId = verify.instanceId;
                    manager.Verify(this);
                }
                else
                    manager.RemoveUnverifiedConnection(this);
                return;
            }

            switch (packet.Type)
            {
                case InPacketType.PlayerCount:
                    var playerCount = (InPlayerCount)packet;
                    overworldPortal.world.PushTickAction(() =>
                    {
                        if (overworldPortal == null) return;
                        overworldPortal.worldName.Value = $"{portalName} ({playerCount.count}/{NetConstants.Max_Overworld_Players})";
                    });
                    manager.PlayerCountUpdate(instanceId, playerCount.count);
                    break;
                case InPacketType.OverworldClosed:
                    manager.InstanceOverworldClosed(this);
                    break;
            }
        }

        public void RequestWorldKey(ulong accountId, uint worldId, Action<string, ulong> callback)
        {
            //Log.Write("Requesting world key...");
            SendTokenAsync(new InWorldKeyRequest(accountId, worldId), (packet, c) =>
            {
                var result = (InWorldKeyResult)packet;
                //Log.Write("Received world key: " + result.key);
                callback?.Invoke(RemoteEndPoint.Address.ToString(), result.key);
            });
        }

        protected override void OnDisconnect()
        {
            manager.InstanceDisconnected(this);
        }
    }
}
