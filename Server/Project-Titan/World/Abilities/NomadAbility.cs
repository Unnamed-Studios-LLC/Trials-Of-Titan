using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using TitanCore.Data;
using TitanCore.Net.Packets.Server;
using Utils.NET.Geometry;
using World.GameState;
using World.Map.Objects.Abilities;
using World.Map.Objects.Entities;

namespace World.Abilities
{
    public class NomadAbility : ClassAbility
    {
        public override ClassType ClassType => ClassType.Nomad;

        private uint assignCooldown;

        public override void OnHit(EntityState entity, uint time, ref int damageTaken)
        {
            if (entity.gameId == PlayerState.currentSnapshot.target)
            {
                PlayerState.currentSnapshot.target = 0;
                player.target.Value = 0;
                assignCooldown = time + 5000;
                PlayerState.AddRage(time, 5);
                damageTaken *= 2;
            }
        }

        public override void OnMove(Vec2 position, uint time)
        {
            /*
            if (time < assignCooldown) return;
            if (player.target.Value == 0)
            {
                AssignTarget();
            }
            */
        }

        private void AssignTarget()
        {
            assignCooldown += 5000;
            if (player.quest != null && player.quest is Enemy questEnemy && questEnemy.DistanceTo(player) < 24)
            {
                player.target.Value = questEnemy.gameId;
                return;
            }
            else
            {
                Enemy strongest = null;
                int hp = 0;

                foreach (var closeEnemy in player.world.objects.GetEnemiesWithin(player.position.Value.x, player.position.Value.y, 20))
                {
                    if (closeEnemy.maxHealth.Value > hp)
                    {
                        hp = closeEnemy.maxHealth.Value;
                        strongest = closeEnemy;
                    }
                }

                if (strongest != null)
                {
                    player.target.Value = strongest.gameId;
                    return;
                }
            }
        }

        public override TnPlayEffect UseAbility(uint time, Vec2 position, Vec2 target, byte value, int attack, ref byte rage, out byte rageCost, out bool sendToSelf, out bool failedToUse)
        {
            failedToUse = false;
            rageCost = AbilityFunctions.Nomad.Ability_Cost;
            sendToSelf = false;

            if (rage < rageCost)
            {
                failedToUse = true;
                return null;
            }

            var charm = new NomadCharm((float)player.world.time.totalTime);
            charm.position.Value = target;
            charm.Initialize(GameData.objects[0xa9d]);
            player.world.objects.SpawnObject(charm, AbilityFunctions.Nomad.Charm_Air_Time);

            rage -= AbilityFunctions.Nomad.Ability_Cost;
            return new TnPlayEffect(new NomadAbilityWorldEffect(player.gameId, target));
        }
    }
}
