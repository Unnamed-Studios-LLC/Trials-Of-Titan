using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Data;
using TitanCore.Net.Packets.Client;
using TitanCore.Net.Packets.Models;
using World.GameState;
using World.Map.Objects.Entities;
using World.Map.Objects.Interfaces;

namespace World.Map.Objects.Map
{
    public class Portal : GameObject, IInteractable
    {
        public override GameObjectType Type => GameObjectType.Portal;

        public override bool Ticks => false;

        private string remoteServer;

        private uint worldId = 0;

        public ObjectStat<string> worldName = new ObjectStat<string>(ObjectStatType.Name, ObjectStatScope.Public, "", "");

        public Portal(uint worldId)
        {
            this.worldId = worldId;
        }

        public Portal(string remoteServer, uint worldId)
        {
            this.remoteServer = remoteServer;
            this.worldId = worldId;
        }

        protected override void GetStats(List<ObjectStat> list)
        {
            base.GetStats(list);

            list.Add(worldName);
        }

        public void Interact(Player player, TnInteract interact)
        {
            player.client.BeginTransferPlayer(remoteServer, worldId);
        }
    }
}
