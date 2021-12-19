using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using TitanCore.Net.Packets.Server;
using Utils.NET.Geometry;
using World.GameState;

namespace World.Abilities
{
    public class BerserkerAbility : ClassAbility
    {
        public override ClassType ClassType => ClassType.Berserker;

        public override void OnHit(EntityState entity, uint time, ref int damageTaken)
        {

        }

        public override void OnMove(Vec2 position, uint time)
        {

        }

        public override TnPlayEffect UseAbility(uint time, Vec2 position, Vec2 target, byte value, int attack, ref byte rage, out byte rageCost, out bool sendToSelf, out bool failedToUse)
        {
            sendToSelf = false;
            rageCost = rage;
            failedToUse = false;

            float shoutSpread = AbilityFunctions.Berserker.GetShoutSpread(rage, attack);
            float shoutRadius = AbilityFunctions.Berserker.GetShoutRange(rage, attack);

            float shoutAngle = position.AngleTo(target);

            var effect = AbilityFunctions.Berserker.GetShoutEffect(rage, attack);
            foreach (var enemy in player.world.objects.GetEnemiesWithin(position.x, position.y, shoutRadius))
            {
                if (Math.Abs(AngleUtils.Difference(position.AngleTo(enemy.position.Value), shoutAngle)) > shoutSpread / 2) continue; // not within the shout cone
                enemy.AddEffect(effect.type, effect.duration);
            }

            var worldEffectPacket = new TnPlayEffect(new BerserkerAbilityWorldEffect(player.gameId, position, shoutAngle * AngleUtils.Rad2Deg, rage, attack));
            rage = 0;
            return worldEffectPacket;
        }
    }
}
