using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Data.Entities;
using TitanCore.Net;

namespace TitanCore.Core
{
    public static class StatFunctions
    {
        public static float TilesPerSecond(int speed, bool slowed, bool speedy)
        {
            var tps = 4f + (speed / 50.0f) * 4f;
            if (slowed)
                tps *= 0.5f;
            if (speedy)
                tps *= 1.5f;
            return tps;
        }

        public static float AttackModifier(int attack, bool damaging)
        {
            var modifier = 0.5f + (attack / 60.0f);
            if (damaging)
                modifier *= 1.5f;
            return modifier;
        }

        public static int DamageTaken(int defense, int damage, bool fortified)
        {
            if (fortified)
                defense *= 2;
            int min = damage / 5;
            int taken = damage - defense;
            return taken < min ? min : taken;
        }

        public static float HealthRegen(int vigor, int timeMs, bool healing, bool sick)
        {
            if (sick) return 0;
            float healingPerSecond = (2 + (vigor / 50.0f) * 10);
            if (healing)
                healingPerSecond += 20;
            return healingPerSecond * (timeMs / 1000f);
        }

        public static int GetLevelUpCost(CharacterInfo info, StatType type, int currentStat, int change)
        {
            int cost = 0;
            int max = info.stats[type].maxValue;
            for (int i = currentStat; i < currentStat + change; i++)
            {
                if (i < max)
                    cost += i * 4;
                else
                {
                    float toPower = (i - (max - 4)) / 54f;
                    cost += (int)(1_000_000 * (toPower * toPower * toPower) + 600 - 6);
                }
            }
            if (type == StatType.MaxHealth)
                cost *= 2;
            return cost;
        }

        public static int GetAscensionCost(StatType type, int currentStat, int statLock, out Item itemCost)
        {
            itemCost = Item.Blank;
            if (statLock == 0) return -1;
            if (type == StatType.MaxHealth)
            {
                if (currentStat - statLock >= NetConstants.Max_Ascension * 10) return -1;
            }
            else if (currentStat - statLock >= NetConstants.Max_Ascension) return -1;

            switch (type)
            {
                case StatType.MaxHealth:
                    itemCost = new Item(0x2ae);
                    break;
                case StatType.Speed:
                    itemCost = new Item(0x2a2);
                    break;
                case StatType.Attack:
                    itemCost = new Item(0x2a3);
                    break;
                case StatType.Defense:
                    itemCost = new Item(0x2a4);
                    break;
                case StatType.Vigor:
                    itemCost = new Item(0x2a5);
                    break;
            }

            itemCost.count = 4;

            return 40_000;
        }

        public static float AttackSpeedModifier(bool fervent, int rofIncreases)
        {
            float rof = 1;
            rof += rofIncreases * 0.01f;
            if (fervent)
                rof *= 1.5f;
            return rof;
        }
    }
}
