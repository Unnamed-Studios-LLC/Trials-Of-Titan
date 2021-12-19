using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using TitanCore.Net.Packets.Server;
using Utils.NET.Geometry;
using Utils.NET.Logging;
using World.GameState;

namespace World.Abilities
{
    public class WarriorAbility : ClassAbility
    {
        public override ClassType ClassType => ClassType.Warrior;

        public uint abilityEndTime;

        public int abilityHealAmount;

        public uint nextHealTime = 0;

        private bool active = false;

        public override void OnHit(EntityState entity, uint time, ref int damageTaken)
        {
            if (time < nextHealTime || time > abilityEndTime) return;
            nextHealTime = time + 1000;

            var position = player.position.Value;
            var effect = new TnPlayEffect(new WarriorAbilityWorldEffect(player.gameId));

            foreach (var otherPlayer in player.world.objects.GetPlayersWithin(position.x, position.y, AbilityFunctions.Warrior.Heal_Area))
            {
                if (otherPlayer == player) continue;
                otherPlayer.Heal(abilityHealAmount);
                otherPlayer.client.SendAsync(effect);
            }
            player.Heal(abilityHealAmount);
            player.client.SendAsync(effect);
        }

        public override void OnMove(Vec2 position, uint time)
        {
            if (time > abilityEndTime && active)
            {
                active = false;
                player.SetSize(1);
            }
        }

        public override TnPlayEffect UseAbility(uint time, Vec2 position, Vec2 target, byte value, int attack, ref byte rage, out byte rageCost, out bool sendToSelf, out bool failedToUse)
        {
            active = true;
            rageCost = rage;
            abilityEndTime = time + AbilityFunctions.Warrior.GetAbilityDuration(rageCost);
            abilityHealAmount = AbilityFunctions.Warrior.GetHealAmount(rageCost, attack);
            player.SetSize(1.2f);

            sendToSelf = false;
            rage = 0;
            failedToUse = false;
            return null;
        }
    }
}
