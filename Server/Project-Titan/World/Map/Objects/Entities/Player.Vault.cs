using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.Logging;
using World.GameState;
using World.Map.Objects.Map.Containers;

namespace World.Map.Objects.Entities
{
    public partial class Player
    {
        private Vault vault;

        public void SetVault(Vault vault)
        {
            this.vault = vault;
        }

        public Vault GetVault()
        {
            if (vault == null) return null;
            return vault;
        }

        public Vault GetVault(uint gameId)
        {
            if (vault == null || vault.gameId != gameId) return null;
            return vault;
        }

        public void ProcessVault()
        {
            if (vault == null || DistanceTo(vault) > Sight.Player_Sight_Radius) return;
            ProcessObject(vault, ref world.time);
        }

        private void TickVault(ref WorldTime time)
        {
            if (vault == null) return;
            vault.Tick(ref time);
        }
    }
}
