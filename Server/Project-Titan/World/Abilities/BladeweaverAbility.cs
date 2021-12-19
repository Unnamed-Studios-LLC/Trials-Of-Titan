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
    public class BladeweaverAbility : ClassAbility
    {
        public override ClassType ClassType => ClassType.Bladeweaver;

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

            if (value > AbilityFunctions.BladeWeaver.Max_Dash_Rage || value > rage) // illegal amount to use
            {
                player.client.SendAsync(new TnError("Invalid rage use amount!"));
                failedToUse = true;
                return null;
            }

            rage -= value;
            PlayerState.AddDashing(position, target, time, value);
            var worldEffectPacket = new TnPlayEffect(new BladeweaverAbilityWorldEffect(player.gameId));

            var bladeweaverItem = new Item(0x2a8);
            var bladeweaverWeaponInfo = (WeaponInfo)bladeweaverItem.GetInfo();
            var bladeweaverProjData = bladeweaverWeaponInfo.projectiles[0];

            var bladeweaverProjectiles = player.GetProjectiles(bladeweaverItem, bladeweaverProjData, bladeweaverWeaponInfo, player.projIds, player.gameId, position.AngleTo(target), false, time);
            var bwProjDamage = (ushort)AbilityFunctions.BladeWeaver.GetProjectileDamage(value, attack);
            for (int i = 0; i < bladeweaverProjectiles.Length; i++)
            {
                bladeweaverProjectiles[i].damage = bwProjDamage;
            }

            player.gameState.AddPlayerProjectiles(time, position, bladeweaverProjectiles);
            foreach (var otherPlayer in player.playersSentTo)
                if (player != otherPlayer)
                    otherPlayer.gameState.AddAllyProjectiles(bladeweaverProjectiles);

            return worldEffectPacket;
        }
    }
}
