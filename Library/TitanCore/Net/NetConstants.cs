using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TitanCore.Core;
using TitanCore.Data.Entities;
using Utils.NET.Geometry;

namespace TitanCore.Net
{
    public static class NetConstants
    {
        public const string Rsa_Public_Key = "AQEAAGbPNII8lUNpQltrgZjZEUu2WMo6XejB3p3bUoD3AJoIplMyw6CstY25nr1ramu04w2c9q7/AF1B5pXrdJ/LovR4RBrGSkJJqjwbZh7AyWehMKQbfSQN7+3yndutQbV4pJKivxpDGY4yuYfO8l//HzYUDnFdxjBWQFHf0Fk58pMDBwAAAAIAAgA=";

        public const int Max_Chat_Length = 120;

        public const int Game_Connection_Port = 12000;

        public const int Vault_Slot_Cost = 80;

        public const int Max_Vault_Slots = 120;

        public const string Premium_Currency_Name = "Token";

        public const int Client_Delta = 16;

        public const float Wall_Collision_Space = 0.4f;

        public const int Max_Overworld_Players = 75;

        public const string Build_Version = "1.9.0";

        public const string Required_Build_Version = "1.9.0";

        public const int Account_Reward_Goal_1 = 10;

        public const int Account_Reward_Goal_2 = 20;

        public const int Account_Reward_Goal_3 = 30;

        public const int Soulless_Cost_Equip = 500;

        public const int Soulless_Cost_Drain = 200;

        public const int Level_Up_Increments = 12;

        public const int Max_Level = 220;

        public const int Max_Ascension = 10;

        private static char[] allowedNameCharacters = "abcdefghijklmnopqrstuvqxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

        public static int GetCharacterSlotCost(int maxCharacters)
        {
            switch (maxCharacters)
            {
                case 1:
                    return 600;
                case 2:
                    return 800;
                default:
                    return 1000;
            }
        }

        public static int GetLevelUpCost(int increment)
        {
            switch (increment)
            {
                case 0:
                    return 10;
                case 1:
                    return 30;
                case 2:
                    return 55;
                case 3:
                    return 90;
                case 4:
                    return 130;
                case 5:
                    return 180;
                case 6:
                    return 300;
                case 7:
                    return 420;
                case 8:
                    return 600;
                case 9:
                    return 900;
                case 10:
                    return 1200;
                case 11:
                    return 1800;
            }
            return int.MaxValue;
        }

        public static IEnumerable<float> GetProjectileAngles(float shootAngle, float angleGap, int amount)
        {
            float angle = shootAngle - (angleGap * (amount - 1)) / 2;
            for (int i = 0; i < amount; i++)
            {
                yield return angle;
                angle += angleGap;
            }
        }

        public static uint GetAoeExpireTime(uint throwTime, uint delta, float lifetime)
        {
            return throwTime + (uint)Math.Ceiling((lifetime * 1000) / delta) * delta;
        }

        public static ushort GetGravestoneType(int level, out float lifetime)
        {
            lifetime = 0;
            if (level < 30)
            {
                lifetime = 30;
                return 0xa26;
            }
            else if (level < 130)
            {
                lifetime = 120;
                return 0xa27;
            }
            else if (level < 220)
                return 0xa28;
            else if (level < 240)
                return 0xa29;
            else if (level < 270)
                return 0xa2a;
            else
                return 0xa2b;
        }

        public static bool IsValidUsername(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;

            if (name.IndexOf(' ') >= 0) return false;

            if (name.Length > 12) return false;

            foreach (var character in name)
            {
                if (!char.IsLetter(character)) return false;
                if (!allowedNameCharacters.Contains(character)) return false;
            }

            return true;
        }

        public static bool IsValidPassword(string password)
        {
            return password.Length >= 8;
        }

        public static int GetStatReward(CharacterStatistic statistic)
        {
            return 0;
        }

        public static long GetBaseDeathReward(CharacterInfo charInfo, ulong souls)
        {
            var totalSouls = (long)souls;
            var totalFullSouls = totalSouls / 1000;

            var reward = totalFullSouls / 100;
            return reward;
        }

        /*
        private static long GetAscensionTotalCost(CharacterInfo charInfo, StatType statType, int stat)
        {
            var max = charInfo.stats[statType].maxValue;
            long cost = 0;
            for (int i = max; i < stat; i++)
                cost += StatFunctions.GetAscensionCost(charInfo, statType, i, out var itemCost);
            return cost;
        }
        */

        public static bool BuildCanPlay(string buildVersion)
        {
            if (!TryParseBuildString(buildVersion, out var versions))
                return false;
            if (!TryParseBuildString(Required_Build_Version, out var currentVersions))
                return false;

            if (versions[0] < currentVersions[0] || versions[1] < currentVersions[1] || versions[2] < currentVersions[2])
                return false;

            return true;
        }

        public static bool BuildAhead(string buildVersion)
        {
            if (!TryParseBuildString(buildVersion, out var versions))
                return false;
            if (!TryParseBuildString(Build_Version, out var currentVersions))
                return false;

            var buildInt = versions[0] * 100_000 + versions[1] * 1000 + versions[2];
            var currentBuildInt = currentVersions[0] * 100_000 + currentVersions[1] * 1000 + currentVersions[2];

            return buildInt > currentBuildInt;
        }

        public static bool TryParseBuildString(string value, out int[] versions)
        {
            var split = value.Split('.');
            if (split.Length > 3)
            {
                versions = null;
                return false;
            }

            try
            {
                versions = split.Select(_ => int.Parse(_)).ToArray();
            }
            catch
            {
                versions = null;
                return false;
            }

            return true;
        }

    }
}
