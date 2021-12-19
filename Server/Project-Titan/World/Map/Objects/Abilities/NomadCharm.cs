using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using TitanCore.Data;
using TitanCore.Net.Packets.Client;
using World.Map.Objects.Entities;
using World.Map.Objects.Interfaces;

namespace World.Map.Objects.Abilities
{
    public class NomadCharm : GameObject, IInteractable
    {
        public override GameObjectType Type => GameObjectType.NomadCharm;

        public override bool Ticks => true;

        private float expireTime;

        private HashSet<ulong> healed = new HashSet<ulong>();

        public ulong owner;

        public NomadCharm(float worldTime)
        {
            expireTime = worldTime + 15;
        }

        public void Interact(Player player, TnInteract interact)
        {
            if (!healed.Add(player.GetOwnerId())) return;
            player.Heal(120);
            player.AddEffect(StatusEffect.Healing, 6);
            if (player.GetOwnerId() == owner)
                player.AddEffect(StatusEffect.Fervent, 4);
        }

        protected override void DoTick(ref WorldTime time)
        {
            base.DoTick(ref time);

            if (time.totalTime >= expireTime)
            {
                world.objects.RemoveObjectPostLogic(this);
                return;
            }
        }
    }
}
