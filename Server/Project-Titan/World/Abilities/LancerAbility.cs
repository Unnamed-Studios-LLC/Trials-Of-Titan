using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using TitanCore.Data.Items;
using TitanCore.Net.Packets.Server;
using Utils.NET.Geometry;
using World.GameState;

namespace World.Abilities
{
    public class LancerAbility : ClassAbility
    {
        public override ClassType ClassType => ClassType.Lancer;

        public override void OnHit(EntityState entity, uint time, ref int damageTaken)
        {

        }

        public override void OnMove(Vec2 position, uint time)
        {

        }

        public override TnPlayEffect UseAbility(uint time, Vec2 position, Vec2 target, byte value, int attack, ref byte rage, out byte rageCost, out bool sendToSelf, out bool failedToUse)
        {
            sendToSelf = false;
            rageCost = AbilityFunctions.Lancer.Rage_Cost;
            failedToUse = false;

            var lancerItem = new Item(0x2a1);
            var lancerWeaponInfo = (WeaponInfo)lancerItem.GetInfo();
            var lancerProjData = lancerWeaponInfo.projectiles[0];

            var offset = AbilityFunctions.Lancer.GetAngleOffset(player.projIds);
            var projectiles = player.GetProjectiles(lancerItem, lancerProjData, lancerWeaponInfo, player.projIds, player.gameId, position.AngleTo(target) + offset, false, time);
            var proj = projectiles[0];
            proj.damage = (ushort)AbilityFunctions.Lancer.GetProjectileDamage(rage, attack);
            projectiles[0] = proj;

            player.gameState.AddPlayerProjectiles(time, position, projectiles);
            foreach (var otherPlayer in player.playersSentTo)
                if (player != otherPlayer)
                    otherPlayer.gameState.AddAllyProjectiles(projectiles);

            rage -= AbilityFunctions.Lancer.Rage_Cost;
            return null;
        }
    }
}
