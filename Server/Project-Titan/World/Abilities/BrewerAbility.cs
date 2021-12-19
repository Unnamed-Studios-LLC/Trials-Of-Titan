using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using TitanCore.Net.Packets.Server;
using Utils.NET.Geometry;
using World.GameState;

namespace World.Abilities
{
    public class BrewerAbility : ClassAbility
    {
        public override ClassType ClassType => ClassType.Brewer;

        public override void OnHit(EntityState entity, uint time, ref int damageTaken)
        {

        }

        public override void OnMove(Vec2 position, uint time)
        {

        }

        public override TnPlayEffect UseAbility(uint time, Vec2 position, Vec2 target, byte value, int attack, ref byte rage, out byte rageCost, out bool sendToSelf, out bool failedToUse)
        {
            var worldEffectPacket = new TnPlayEffect(new BrewerAbilityWorldEffect(player.gameId, position, rage, attack, value));
            sendToSelf = false;
            rageCost = rage;
            rage = 0;
            failedToUse = false;
            return worldEffectPacket;
        }
    }
}
