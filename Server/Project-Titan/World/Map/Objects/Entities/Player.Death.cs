using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TitanCore.Core;
using TitanCore.Data;
using TitanCore.Data.Entities;
using TitanCore.Net;
using TitanCore.Net.Packets.Models;
using TitanCore.Net.Packets.Server;
using TitanDatabase.Leaderboards;
using Utils.NET.Logging;
using World.Map.Objects.Map;

namespace World.Map.Objects.Entities
{
    public partial class Player
    {
        public void Die(GameObjectInfo damagerInfo)
        {
            if (dead) return;
            dead = true;

            var chat = ChatData.Info($"{playerName.Value} was killed at level {GetLevel()} by {(damagerInfo is EnemyInfo enemyInfo ? enemyInfo.title : damagerInfo.name)}");
            foreach (var player in playersSentTo)
                player.AddChat(chat);

            MakeGravestone();

            var baseReward = GetBaseDeathReward();
            var statistics = character.statistics.Values.ToArray();
            client.SendAsync(new TnDeath(damagerInfo.id, DateTime.UtcNow, baseReward, statistics));

            var reward = new DeathReward(baseReward, statistics);

            character.dead = true;
            character.deathValue = (ulong)reward.totalReward;

            client.account.deathCurrency += reward.totalReward;
            client.account.CharacterDied(character.id);

            LeaderboardManager.PushDeath(character);

            world.LogoutPlayer(this, null);
        }

        private long GetBaseDeathReward()
        {
            var charInfo = (CharacterInfo)info;

            return NetConstants.GetBaseDeathReward(charInfo, GetStatisticValue(CharacterStatisticType.SoulsEarned));
        }

        private void MakeGravestone()
        {
            var grave = new Gravestone(playerName.Value);
            grave.Initialize(GameData.objects[NetConstants.GetGravestoneType(GetLevel(), out grave.lifetime)]);
            grave.position.Value = position.Value;
            world.objects.SpawnObject(grave);
        }
    }
}
