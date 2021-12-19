using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TitanCore.Core;
using TitanCore.Data;
using World.Map.Objects.Entities;

namespace World.Map.Objects.Abilities
{
    public class AlchemistAbilityObject : GameObject
    {
        public override GameObjectType Type => GameObjectType.GroundObject;

        public override bool Ticks => true;

        private AlchemistAbilityWorldEffect effect;

        private float radius;

        private float startTime;

        private float nextTick;

        private float endTime;

        private Player owner;

        private int damage;

        public AlchemistAbilityObject(Player owner, AlchemistAbilityWorldEffect effect, float time)
        {
            this.owner = owner;
            this.effect = effect;
            position.Value = effect.target;
            radius = AbilityFunctions.Alchemist.GetRadius(effect.rage);
            startTime = time + AbilityFunctions.Alchemist.Air_Time;
            endTime = startTime + AbilityFunctions.Alchemist.GetGroundDurationMs(effect.rage) / 1000f;

            damage = (int)(effect.rage + effect.attack);
        }

        public override bool CanShowTo(Player player)
        {
            return false;
        }

        protected override void DoTick(ref WorldTime time)
        {
            base.DoTick(ref time);

            if (time.totalTime < startTime) return;

            if (time.totalTime >= endTime)
            {
                world.objects.RemoveObjectPostLogic(this);
                return;
            }

            if (time.totalTime < nextTick) return;

            foreach (var player in world.objects.GetPlayersWithin(position.Value.x, position.Value.y, radius).ToArray())
            {
                player.AddEffect(StatusEffect.Damaging, 1.05f);
            }

            foreach (var enemy in world.objects.GetEnemiesWithin(position.Value.x, position.Value.y, radius).ToArray())
            {
                var damageTaken = enemy.GetDamageTaken(damage);
                enemy.Hurt(damageTaken, owner);
                enemy.ServerDamage(damage, owner.info);
                owner.OnDamageEnemy(enemy, damageTaken);
                if (enemy.GetHealth() <= 0)
                {
                    world.PushTickAction(() =>
                    {
                        enemy.Die(owner);
                    });
                }
            }

            nextTick = (float)time.totalTime + 1f;
        }
    }
}
