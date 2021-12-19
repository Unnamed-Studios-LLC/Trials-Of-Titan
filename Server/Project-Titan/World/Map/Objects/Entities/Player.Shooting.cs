using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using TitanCore.Data;
using TitanCore.Data.Components.Projectiles;
using TitanCore.Data.Items;
using TitanCore.Net;
using TitanCore.Net.Packets.Client;
using TitanCore.Net.Packets.Models;
using TitanCore.Net.Packets.Server;
using Utils.NET.Geometry;
using Utils.NET.Logging;
using World.Net;

namespace World.Map.Objects.Entities
{
    public partial class Player
    {
        public uint projIds = 0;

        private uint nextShootTime;

        /// <summary>
        /// Processes a shot packet into the player's projectile
        /// </summary>
        /// <param name="shoot"></param>
        public void ProcessShoot(TnShoot shoot)
        {
            var time = shoot.clientTickId * Client.Client_Fixed_Delta;
            if (time < nextShootTime)
            {
                client.SendAsync(new TnError("Attack speed check failed!"));
                return;
            }

            if (gameState.playerState == null) return;

            if (!gameState.playerState.AdvancePosition(shoot.position, time))
            {
                client.SendAsync(new TnError("Walk failure! Shoot"));
                return;
            }

            var item = gameState.playerState.currentSnapshot.equips[0];
            if (item.IsBlank) return;
            var itemInfo = item.GetInfo();
            if (!(itemInfo is WeaponInfo weaponInfo)) return;

            var shootCooldown = (int)(1000 / (weaponInfo.rateOfFire * StatFunctions.AttackSpeedModifier(gameState.playerState.HasEffect(StatusEffect.Fervent, time), gameState.playerState.currentSnapshot.GetAlternateStat(AlternateStatType.RateOfFire))));
            nextShootTime = (uint)(time + shootCooldown);

            var startProjectileId = projIds;
            var projData = weaponInfo.projectiles[startProjectileId % weaponInfo.projectiles.Length];

            if (projData.Type == ProjectileType.Aoe)
            {
                var projectiles = GetAoeProjectiles(item, projData, weaponInfo, startProjectileId, gameId, shoot.target, shoot.position, time);

                gameState.AddPlayerAoeProjectiles(shoot.clientTickId * Client.Client_Fixed_Delta, shoot.position, projectiles);
                foreach (var player in playersSentTo)
                    if (player != this)
                        player.gameState.AddAllyAoeProjectiles(projectiles);
            }
            else
            {
                var projectiles = GetProjectiles(item, projData, weaponInfo, startProjectileId, gameId, shoot.position.AngleTo(shoot.target), gameState.playerState.HasEffect(StatusEffect.Reach, time), time);

                gameState.AddPlayerProjectiles(shoot.clientTickId * Client.Client_Fixed_Delta, shoot.position, projectiles);
                foreach (var player in playersSentTo)
                    if (player != this)
                        player.gameState.AddAllyProjectiles(projectiles);
            }
        }

        public AllyProjectile[] GetProjectiles(Item item, ProjectileData projData, WeaponInfo weaponInfo, uint projectileId, uint ownerId, float angle, bool reach, uint time)
        {
            var projectiles = new AllyProjectile[projData.amount];
            int i = 0;
            foreach (var projAngle in NetConstants.GetProjectileAngles(angle, projData.angleGap, projData.amount))
            {
                uint id = projectileId++;
                projData = weaponInfo.projectiles[id % weaponInfo.projectiles.Length];
                projectiles[i++] = new AllyProjectile()
                {
                    ownerId = ownerId,
                    projectileId = id,
                    damage = item.enchantType == ItemEnchantType.Damaging ? ApplyDamageEnchantment(item.enchantLevel, GetDamage(weaponInfo.slotType, projData, id, time)) : GetDamage(weaponInfo.slotType, projData, id, time),
                    item = weaponInfo.id,
                    angle = projAngle,
                    reach = reach
                };
            }
            projIds += (uint)projectiles.Length;
            return projectiles;
        }

        private AllyAoeProjectile[] GetAoeProjectiles(Item item, ProjectileData projData, WeaponInfo weaponInfo, uint projectileId, uint ownerId, Vec2 target, Vec2 start, uint time)
        {
            var projectiles = new AllyAoeProjectile[projData.amount];
            int i = 0;
            var length = start.DistanceTo(target);
            foreach (var angle in NetConstants.GetProjectileAngles(start.AngleTo(target), projData.angleGap, projData.amount))
            {
                uint id = projectileId++;
                projectiles[i++] = new AllyAoeProjectile()
                {
                    ownerId = ownerId,
                    projectileId = id,
                    damage = item.enchantType == ItemEnchantType.Damaging ? ApplyDamageEnchantment(item.enchantLevel, GetDamage(weaponInfo.slotType, projData, id, time)) : GetDamage(weaponInfo.slotType, projData, id, time),
                    item = weaponInfo.id,
                    target = start + Vec2.FromAngle(angle) * length
                };
            }
            projIds += (uint)projectiles.Length;
            return projectiles;
        }

        private ushort ApplyDamageEnchantment(int level, ushort damage)
        {
            return (ushort)(damage * EnchantFunctions.Damage(level));
        }

        private ushort GetDamage(SlotType slotType, ProjectileData data, uint id, uint time)
        {
            var rand = world.RandValue(id);

            WeaponFunctions.GetProjectileDamage(slotType, data, out var minDamage, out var maxDamage);
            return (ushort)((minDamage + (ushort)((maxDamage - minDamage) * rand)) * StatFunctions.AttackModifier(gameState.playerState.currentSnapshot.GetFunctionalStat(StatType.Attack), gameState.playerState.HasEffect(StatusEffect.Damaging, time)));
        }
    }
}
