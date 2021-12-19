using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TitanCore.Core;
using TitanCore.Data.Entities;
using TitanCore.Net.Packets.Server;
using Utils.NET.Geometry;
using Utils.NET.Logging;
using World.GameState;

namespace World.Abilities
{
    public class RangerAbility : ClassAbility
    {
        public override ClassType ClassType => ClassType.Ranger;

        public override void OnHit(EntityState entity, uint time, ref int damageTaken)
        {

        }

        public override void OnMove(Vec2 position, uint time)
        {

        }

        public override TnPlayEffect UseAbility(uint time, Vec2 position, Vec2 target, byte value, int attack, ref byte rage, out byte rageCost, out bool sendToSelf, out bool failedToUse)
        {
            rageCost = rage;
            failedToUse = false;

            var targetVec = target - position;
            var curLength = targetVec.Length;
            if (curLength > 6)
                target = position + targetVec.ChangeLength(6, curLength);

            var rangerRadius = AbilityFunctions.Ranger.GetRadius(rage, attack);
            var rangerDamage = AbilityFunctions.Ranger.GetDamage(rage, attack);
            var rangerEffect = AbilityFunctions.Ranger.GetEffect(rage, attack);

            var hit = new List<uint>();

            foreach (var enemy in player.world.objects.GetEnemiesWithin(target.x, target.y, rangerRadius).ToArray())
            {
                // return ((EntityInfo)info).invincible || HasStatusEffect(StatusEffect.Invincible) || HasStatusEffect(StatusEffect.KnockedBack) || HasStatusEffect(StatusEffect.Grounded);
                if (((EntityInfo)enemy.info).invincible || enemy.HasServerEffect(StatusEffect.Invincible) || enemy.HasServerEffect(StatusEffect.Invulnerable)) continue;

                hit.Add(enemy.gameId);
                var damageTaken = enemy.GetDamageTaken(rangerDamage);
                enemy.Hurt(damageTaken, player);
                player.OnDamageEnemy(enemy, damageTaken);
                if (enemy.GetHealth() <= 0)
                    enemy.Die(player);
                if (rangerEffect.HasValue)
                    enemy.AddEffect(rangerEffect.Value.type, rangerEffect.Value.duration);
                if (hit.Count == 255) break;
            }

            var worldEffectPacket = new TnPlayEffect(new RangerAbilityWorldEffect(hit.ToArray(), target, rage, attack));
            sendToSelf = true;
            rage = 0;
            return worldEffectPacket;
        }
    }
}
