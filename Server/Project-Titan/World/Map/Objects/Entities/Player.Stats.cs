using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using TitanCore.Data;
using TitanCore.Data.Components;
using TitanCore.Data.Entities;
using TitanCore.Net;
using TitanCore.Net.Packets.Models;
using TitanCore.Net.Packets.Server;
using Utils.NET.Logging;
using World.GameState;

namespace World.Map.Objects.Entities
{
    public partial class Player
    {
        private ulong souls;

        public byte statLevelIncrement;

        public ObjectStat<int> fullSouls = new ObjectStat<int>(ObjectStatType.Souls, ObjectStatScope.Public, 0, 0);

        public ObjectStat<int> soulGoal = new ObjectStat<int>(ObjectStatType.SoulGoal, ObjectStatScope.Private, 0, 0);

        public ObjectStat<int> maxHealth = new ObjectStat<int>(ObjectStatType.MaxHealth, ObjectStatScope.Public, 0, 0);

        public ObjectStat<int> health = new ObjectStat<int>(ObjectStatType.Health, ObjectStatScope.Public, 0, 0);

        public ObjectStat<int> speed = new ObjectStat<int>(ObjectStatType.Speed, ObjectStatScope.Public, 0, 0);

        public ObjectStat<int> attack = new ObjectStat<int>(ObjectStatType.Attack, ObjectStatScope.Public, 0, 0);

        public ObjectStat<int> defense = new ObjectStat<int>(ObjectStatType.Defense, ObjectStatScope.Public, 0, 0);

        public ObjectStat<int> vigor = new ObjectStat<int>(ObjectStatType.Vigor, ObjectStatScope.Public, 0, 0);

        public ObjectStat<int> maxHealthBonus = new ObjectStat<int>(ObjectStatType.MaxHealthBonus, ObjectStatScope.Public, 0, 0);

        public ObjectStat<int> speedBonus = new ObjectStat<int>(ObjectStatType.SpeedBonus, ObjectStatScope.Public, 0, 0);

        public ObjectStat<int> attackBonus = new ObjectStat<int>(ObjectStatType.AttackBonus, ObjectStatScope.Public, 0, 0);

        public ObjectStat<int> defenseBonus = new ObjectStat<int>(ObjectStatType.DefenseBonus, ObjectStatScope.Public, 0, 0);

        public ObjectStat<int> vigorBonus = new ObjectStat<int>(ObjectStatType.VigorBonus, ObjectStatScope.Public, 0, 0);

        public ObjectStat<int> maxHealthLock = new ObjectStat<int>(ObjectStatType.MaxHealthLock, ObjectStatScope.Private, 0, 0);

        public ObjectStat<int> speedLock = new ObjectStat<int>(ObjectStatType.SpeedLock, ObjectStatScope.Private, 0, 0);

        public ObjectStat<int> attackLock = new ObjectStat<int>(ObjectStatType.AttackLock, ObjectStatScope.Private, 0, 0);

        public ObjectStat<int> defenseLock = new ObjectStat<int>(ObjectStatType.DefenseLock, ObjectStatScope.Private, 0, 0);

        public ObjectStat<int> vigorLock = new ObjectStat<int>(ObjectStatType.VigorLock, ObjectStatScope.Private, 0, 0);

        /// <summary>
        /// The amount of increase each stat has from equipment
        /// </summary>
        private Dictionary<StatType, int> statIncreases = new Dictionary<StatType, int>();

        public GameObjectInfo lastServerDamager;


        private void LoadStats()
        {
            var charInfo = (CharacterInfo)info;

            maxHealth.Value = (int)character.stats[(int)StatType.MaxHealth];
            speed.Value = (int)character.stats[(int)StatType.Speed];
            attack.Value = (int)character.stats[(int)StatType.Attack];
            defense.Value = (int)character.stats[(int)StatType.Defense];
            vigor.Value = (int)character.stats[(int)StatType.Vigor];

            if (character.statsLocked.Count > 0)
            {
                maxHealthLock.Value = (int)character.statsLocked[(int)StatType.MaxHealth];
                speedLock.Value = (int)character.statsLocked[(int)StatType.Speed];
                attackLock.Value = (int)character.statsLocked[(int)StatType.Attack];
                defenseLock.Value = (int)character.statsLocked[(int)StatType.Defense];
                vigorLock.Value = (int)character.statsLocked[(int)StatType.Vigor];
            }

            statLevelIncrement = character.levelIncrement;
            health.Value = GetStatFunctional(StatType.MaxHealth);

            SetSouls(character.souls);

            UpdateGoalBar();
        }

        private void SaveStats()
        {
            character.stats[(int)StatType.MaxHealth] = (uint)maxHealth.Value;
            character.stats[(int)StatType.Speed] = (uint)speed.Value;
            character.stats[(int)StatType.Attack] = (uint)attack.Value;
            character.stats[(int)StatType.Defense] = (uint)defense.Value;
            character.stats[(int)StatType.Vigor] = (uint)vigor.Value;

            character.souls = souls;
            character.levelIncrement = statLevelIncrement;
        }

        private ObjectStat<int> GetStatObject(StatType type)
        {
            switch (type)
            {
                case StatType.Speed:
                    return speed;
                case StatType.Attack:
                    return attack;
                case StatType.Defense:
                    return defense;
                case StatType.Vigor:
                    return vigor;
                default:
                    return maxHealth;
            }
        }

        /// <summary>
        /// Returns the player's base stat value of a given type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public int GetStatBase(StatType type)
        {
            return GetStatObject(type).Value;
        }

        /// <summary>
        /// Returns the player's stat lock value of a given type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public int GetStatLock(StatType type)
        {
            switch (type)
            {
                case StatType.MaxHealth:
                    return maxHealthLock.Value;
                case StatType.Speed:
                    return speedLock.Value;
                case StatType.Attack:
                    return attackLock.Value;
                case StatType.Defense:
                    return defenseLock.Value;
                case StatType.Vigor:
                    return vigorLock.Value;
            }
            return 0;
        }

        /// <summary>
        /// Sets the player's base stat of the given type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        public void SetStatBase(StatType type, int value)
        {
            GetStatObject(type).Value = value;
        }

        /// <summary>
        /// Returns the amount a stat is increased
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private int GetStatIncrease(StatType type)
        {
            if (statIncreases.TryGetValue(type, out var stat))
                return stat;
            return 0;
        }

        private ObjectStat<int> GetStatBonusObject(StatType type)
        {
            switch (type)
            {
                case StatType.MaxHealth:
                    return maxHealthBonus;
                case StatType.Speed:
                    return speedBonus;
                case StatType.Attack:
                    return attackBonus;
                case StatType.Defense:
                    return defenseBonus;
                case StatType.Vigor:
                    return vigorBonus;
            }
            return null;
        }

        /// <summary>
        /// Gets the current stat bonus for a given stat type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private int GetStatBonus(StatType type)
        {
            return GetStatBonusObject(type).Value;
        }

        /// <summary>
        /// Returns the base stat plus any stat increase, use this for stat functions
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public int GetStatFunctional(StatType type)
        {
            return GetStatBase(type) + GetStatIncrease(type) + GetStatBonus(type);
        }

        public void AddStatBonus(StatType type, int amount)
        {
            var obj = GetStatBonusObject(type);
            obj.Value += amount;
        }

        public void RemoveStatBonus(StatType type, int amount)
        {
            var obj = GetStatBonusObject(type);
            obj.Value -= amount;
        }

        private void SetSouls(ulong souls)
        {
            this.souls = souls;

            int full = (int)(souls / 1000);

            fullSouls.Value = full;

            TryLevelUp();
        }

        public void GiveSouls(ulong amount)
        {
            AddSoulsStatistic(amount);
            SetSouls(souls + amount);
        }

        public void AddFullSouls(ulong amount)
        {
            AddSoulsStatistic(amount * 1000);
            SetSouls(souls + amount * 1000);
        }

        public void RemoveFullSouls(ulong amount)
        {
            SetSouls(souls - amount * 1000);
        }

        public override void Heal(int amount)
        {
            if (amount <= 0) return;
            
            base.Heal(amount);

            //health.Value += amount;
            //health.Value = Math.Min(health.Value + amount, GetStatFunctional(StatType.MaxHealth));
        }

        public override void ServerDamage(int amount, GameObjectInfo damager)
        {
            base.ServerDamage(amount, damager);

            lastServerDamager = damager;
        }

        public override void Hurt(int damage, Entity damager)
        {

        }

        public void OnDamageEnemy(Enemy enemy, int damage)
        {
            if (enemy == quest)
                AddQuestDamage(damage);
        }

        private void CompleteClassQuest(int index)
        {
            client.CompleteClassQuest((ClassType)info.id, index);
            currentClassQuest = client.account.GetClassQuest((ClassType)info.id);
            classQuests.Value += 1;
            AddChat(ChatData.Info("You have completed a class quest!"));

            TryGiveMastery();
        }

        private void TryGiveMastery()
        {
            for (int i = 0; i < 4; i++)
                if (!currentClassQuest.HasCompletedQuest(i))
                    return;

            // give mastery
            //AddChat(ChatData.Info($"You've completed all class quests! {info.name} class mastery skin has been added to your wardrobe."));
        }

        private void AddSoulsStatistic(ulong amount)
        {
            var newValue = AddStatistic(CharacterStatisticType.SoulsEarned, amount);
            if (!currentClassQuest.HasCompletedQuest(0))
            {
                if (newValue >= 10_000_000)
                {
                    CompleteClassQuest(0);
                }
            }
            if (!currentClassQuest.HasCompletedQuest(1))
            {
                if (newValue >= 25_000_000)
                {
                    CompleteClassQuest(1);
                }
            }
        }

        public ulong AddStatistic(CharacterStatisticType type, ulong value)
        {
            if (!character.statistics.TryGetValue(type, out var stat))
                stat = new CharacterStatistic(type, 0);
            stat.value += value;
            character.statistics[type] = stat;
            return stat.value;
        }

        public ulong IncrementStatistic(CharacterStatisticType type)
        {
            if (!character.statistics.TryGetValue(type, out var stat))
                stat = new CharacterStatistic(type, 0);
            stat.value++;
            character.statistics[type] = stat;
            return stat.value;
        }

        public ulong GetStatisticValue(CharacterStatisticType type)
        {
            if (!character.statistics.TryGetValue(type, out var stat))
                return 0;
            return stat.value;
        }

        private void UpdateGoalBar()
        {
            if (statLevelIncrement < NetConstants.Level_Up_Increments)
                soulGoal.Value = NetConstants.GetLevelUpCost(statLevelIncrement);
            else
                soulGoal.Value = 0;
        }

        private void TryLevelUp()
        {
            if (statLevelIncrement >= NetConstants.Level_Up_Increments) return;

            var cost = NetConstants.GetLevelUpCost(statLevelIncrement);
            if (fullSouls.Value >= cost)
            {
                LevelUp();
                TakeEssence(cost);
            }
        }

        public void LevelUp()
        {
            if (statLevelIncrement >= NetConstants.Level_Up_Increments) return;

            var charInfo = (CharacterInfo)info;
            foreach (var stat in charInfo.stats.Values)
            {
                IncreaseStat(stat.type, stat.increaseLoop[statLevelIncrement % stat.increaseLoop.Length]);
            }

            statLevelIncrement++;

            LeveledUp();
            LevelUpEffect();
        }

        public void LeveledUp()
        {
            var level = GetLevel();
            if (!currentClassQuest.HasCompletedQuest(2) && level >= 160)
            {
                CompleteClassQuest(2);
            }

            if (!currentClassQuest.HasCompletedQuest(3) && level >= 200)
            {
                CompleteClassQuest(3);
            }

            UpdateGoalBar();
        }

        public bool IncreaseStat(StatType type, int amount)
        {
            var level = GetLevel();
            var allowedLevels = NetConstants.Max_Level - level;
            amount = Math.Min(amount, allowedLevels);

            if (amount <= 0) return false;

            var charInfo = (CharacterInfo)info;
            var baseStat = GetStatBase(type);
            var statData = charInfo.stats[type];

            if (baseStat == statData.maxValue) return false;

            if (type == StatType.MaxHealth)
                amount = Math.Min((statData.maxValue - baseStat) / 10, amount);
            else
                amount = Math.Min(statData.maxValue - baseStat, amount);

            if (amount <= 0) return false;

            if (type == StatType.MaxHealth)
                baseStat += amount * 10;
            else
                baseStat += amount;
            SetStatBase(type, baseStat);

            level += amount;
            if (level == NetConstants.Max_Level)
            {
                LockStats();
            }

            LeveledUp();

            return true;
        }

        private void LevelUpEffect()
        {
            var effectPacket = new TnPlayEffect(new LevelUpWorldEffect(gameId));
            foreach (var otherPlayer in playersSentTo)
            {
                if (otherPlayer.DistanceTo(this) >= 20) continue;
                otherPlayer.client.SendAsync(effectPacket);
            }
        }

        private void LockStats()
        {
            maxHealthLock.Value = maxHealth.Value;
            speedLock.Value = speed.Value;
            attackLock.Value = attack.Value;
            defenseLock.Value = defense.Value;
            vigorLock.Value = vigor.Value;
            
            character.statsLocked = new List<uint>()
            {
                (uint)maxHealthLock.Value,
                (uint)speedLock.Value,
                (uint)attackLock.Value,
                (uint)defenseLock.Value,
                (uint)vigorLock.Value
            };
        }
    }
}
