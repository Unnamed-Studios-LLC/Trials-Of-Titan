using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using TitanCore.Data.Entities;
using TitanCore.Net.Packets.Client;
using TitanCore.Net.Packets.Models;
using TitanCore.Net.Packets.Server;

namespace World.Net.Handling
{
    public class LevelUpHandler : ClientPacketHandler<TnLevelUp>
    {
        public override void Handle(TnLevelUp packet, Client connection)
        {
            return;

            var speedBase = connection.player.GetStatBase(StatType.Speed);
            var attackBase = connection.player.GetStatBase(StatType.Attack);
            var defenseBase = connection.player.GetStatBase(StatType.Defense);
            var vigorBase = connection.player.GetStatBase(StatType.Vigor);
            var maxHealthBase = connection.player.GetStatBase(StatType.MaxHealth);

            var info = (CharacterInfo)connection.player.info;

            if (packet.speedIncrease == 0 && packet.attackIncrease == 0 && packet.defenseIncrease == 0 && packet.vigorIncrease == 0 && packet.maxHealthIncrease == 0)
            {
                return;
            }

            if ((packet.speedIncrease != 0 && speedBase + packet.speedIncrease > info.stats[StatType.Speed].maxValue) ||
                (packet.attackIncrease != 0 && attackBase + packet.attackIncrease > info.stats[StatType.Attack].maxValue) ||
                (packet.defenseIncrease != 0 && defenseBase + packet.defenseIncrease > info.stats[StatType.Defense].maxValue) ||
                (packet.vigorIncrease != 0 && vigorBase + packet.vigorIncrease > info.stats[StatType.Vigor].maxValue) ||
                (packet.maxHealthIncrease != 0 && maxHealthBase + packet.maxHealthIncrease * 10 > info.stats[StatType.MaxHealth].maxValue))
            {
                connection.player.AddChat(ChatData.Error("Cannot level a stat past max through this method!"));
                return;
            }

            var speedCost = StatFunctions.GetLevelUpCost(info, StatType.Speed, speedBase, packet.speedIncrease);
            var attackCost = StatFunctions.GetLevelUpCost(info, StatType.Attack, attackBase, packet.attackIncrease);
            var defenseCost = StatFunctions.GetLevelUpCost(info, StatType.Defense, defenseBase, packet.defenseIncrease);
            var vigorCost = StatFunctions.GetLevelUpCost(info, StatType.Vigor, vigorBase, packet.vigorIncrease);
            var maxHealthCost = StatFunctions.GetLevelUpCost(info, StatType.MaxHealth, maxHealthBase / 10 - 5, packet.maxHealthIncrease);

            var costTotal = speedCost + attackCost + defenseCost + vigorCost + maxHealthCost;

            if (costTotal < 0) // invalid level up
            {
                return;
            }

            var currentSouls = connection.player.fullSouls.Value;

            if (currentSouls < costTotal)
            {
                connection.player.AddChat(ChatData.Error("Not enough souls to perform level up!"));
                return;
            }

            connection.player.RemoveFullSouls((ulong)costTotal);

            connection.player.SetStatBase(StatType.Speed, speedBase + packet.speedIncrease);
            connection.player.SetStatBase(StatType.Attack, attackBase + packet.attackIncrease);
            connection.player.SetStatBase(StatType.Defense, defenseBase + packet.defenseIncrease);
            connection.player.SetStatBase(StatType.Vigor, vigorBase + packet.vigorIncrease);
            connection.player.SetStatBase(StatType.MaxHealth, maxHealthBase + packet.maxHealthIncrease * 10);

            connection.player.LeveledUp();
        }
    }
}
