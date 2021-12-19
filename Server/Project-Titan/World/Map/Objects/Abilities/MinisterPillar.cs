using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using TitanCore.Data;
using TitanCore.Net.Packets.Server;
using Utils.NET.Collections;
using Utils.NET.Geometry;
using World.Map.Objects.Entities;

namespace World.Map.Objects.Abilities
{
    public class MinisterPillar : GameObject
    {
        public override GameObjectType Type => GameObjectType.StaticObject;

        public override bool Ticks => true;

        private float radius;

        private float startTime;

        private float endTime;

        private int healAmount;

        private ExpirationQueue<uint> healedExpiration;

        private HashSet<uint> healed = new HashSet<uint>();

        public MinisterPillar(int rage, int attack, Vec2 position, float time)
        {
            this.position.Value = position;
            radius = AbilityFunctions.Minister.GetPillarRadius(rage);
            startTime = time + 0.5f;
            endTime = startTime + AbilityFunctions.Minister.GetPillarDurationMs(rage) / 1000f;
            healAmount = AbilityFunctions.Minister.GetHealAmount(rage, attack);

            healedExpiration = new ExpirationQueue<uint>(2);
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

            foreach (var p in healedExpiration.GetExpired())
                healed.Remove(p);

            foreach (var player in world.objects.GetPlayersWithin(position.Value.x, position.Value.y, radius))
            {
                Heal(player);
                /*
                if (player.health.Value < player.GetStatFunctional(StatType.MaxHealth))
                {
                    Heal(player);
                }
                */
            }
        }

        private void Heal(Player player)
        {
            if (!healed.Add(player.gameId)) return;
            healedExpiration.Enqueue(player.gameId);
            player.Heal(healAmount);

            var pkt = new TnPlayEffect(new HealLaserWorldEffect(gameId, player.gameId));
            foreach (var p in player.playersSentTo)
                p.client.SendAsync(pkt);
        }
    }
}
