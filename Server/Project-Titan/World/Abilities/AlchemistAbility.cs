using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using TitanCore.Data;
using TitanCore.Net.Packets.Server;
using Utils.NET.Geometry;
using World.GameState;
using World.Map.Objects.Abilities;

namespace World.Abilities
{
    public class AlchemistAbility : ClassAbility
    {
        public override ClassType ClassType => ClassType.Alchemist;

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

            var alchemistEffect = new AlchemistAbilityWorldEffect(player.gameId, target, rage, attack);
            var worldEffectPacket = new TnPlayEffect(alchemistEffect);

            var groundRing = new AlchemistAbilityObject(player, alchemistEffect, (float)player.world.time.totalTime);
            groundRing.Initialize(GameData.objects[0xa2e]);
            player.world.objects.SpawnObject(groundRing);

            rage = 0;
            return worldEffectPacket;
        }
    }
}
