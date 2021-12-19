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
    public class MinisterAbility : ClassAbility
    {
        public override ClassType ClassType => ClassType.Minister;

        public override void OnHit(EntityState entity, uint time, ref int damageTaken)
        {

        }

        public override void OnMove(Vec2 position, uint time)
        {

        }

        public override TnPlayEffect UseAbility(uint time, Vec2 position, Vec2 target, byte value, int attack, ref byte rage, out byte rageCost, out bool sendToSelf, out bool failedToUse)
        {
            sendToSelf = false;
            failedToUse = false;

            var cost = AbilityFunctions.Minister.GetRageCost(rage);
            rageCost = cost;
            if (rage < cost)
            {
                player.client.SendAsync(new TnError("Invalid minister ability usage. Not have enough rage."));
                return null;
            }

            ushort pillarType = 0xa2f;
            if (cost >= 100)
                pillarType = 0xa9c;
            else if (cost >= 75)
                pillarType = 0xa9b;
            else if (cost >= 50)
                pillarType = 0xa9a;

            var pillar = new MinisterPillar(cost, attack, position, (float)player.world.time.totalTime);
            pillar.Initialize(GameData.objects[pillarType]);
            player.world.objects.SpawnObject(pillar);

            var worldEffectPacket = new TnPlayEffect(new MinisterAbilityWorldEffect(player.gameId, position, cost, attack));
            rage -= cost;
            return worldEffectPacket;
        }
    }
}
