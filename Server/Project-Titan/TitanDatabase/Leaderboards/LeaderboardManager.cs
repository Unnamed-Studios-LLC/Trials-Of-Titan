using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TitanCore.Core;
using TitanDatabase.Models;
using Utils.NET.Logging;
using Utils.NET.Utils;

namespace TitanDatabase.Leaderboards
{
    public class LeaderboardManager
    {
        private static LeaderboardType[] leaderboardTypes = (LeaderboardType[])Enum.GetValues(typeof(LeaderboardType));

        private static LeaderboardManager instance;

        public static ManualResetEvent Initialize()
        {
            var resetEvent = new ManualResetEvent(false);
            if (instance != null)
                resetEvent.Set();
            else
                DoInit(resetEvent);
            return resetEvent;
        }

        private static async void DoInit(ManualResetEvent resetEvent)
        {
            instance = await Load();
            resetEvent.Set();
        }

        private static async Task<LeaderboardManager> Load()
        {
            Log.Write("Loading Leaderboards...");

            var list = new ConcurrentBag<Leaderboard>();
            var tasks = new List<Task>();
            foreach (var type in leaderboardTypes)
            {
                tasks.Add(LoadLeaderboard(type, list));
            }
            await Task.WhenAll(tasks);
            Log.Write("Loaded Leaderboards.");
            return new LeaderboardManager(list);
        }

        private static async Task LoadLeaderboard(LeaderboardType type, ConcurrentBag<Leaderboard> bag)
        {
            var response = await Leaderboard.GetOrCreate((ulong)type);
            if (response.result != Model.RequestResult.Success)
                bag.Add(Leaderboard.Default((ulong)type));
            else
                bag.Add(response.item);
        }

        public static void PushDeath(Character death)
        {
            instance.deathQueue.Push(death);
        }

        public static void PushLiving(Character living)
        {
            instance.livingQueue.Push(living);
        }

        public static Task<Leaderboard> Get(LeaderboardType type)
        {
            return instance.GetLeaderboard(type);
        }


        private ConcurrentDictionary<LeaderboardType, Leaderboard> leaderboards = new ConcurrentDictionary<LeaderboardType, Leaderboard>();

        private AsyncQueueProcessor<Character> deathQueue;

        private AsyncQueueProcessor<Character> livingQueue;

        public LeaderboardManager(IEnumerable<Leaderboard> leaderboards)
        {
            foreach (var leaderboard in leaderboards)
            {
                this.leaderboards.TryAdd((LeaderboardType)leaderboard.id, leaderboard);
            }

            deathQueue = new AsyncQueueProcessor<Character>(ProcessDeath);
            livingQueue = new AsyncQueueProcessor<Character>(ProcessLiving);
        }

        private async Task<Leaderboard> GetLeaderboard(LeaderboardType type)
        {
            var leaderboard = leaderboards[type];
            if (leaderboard.Outdated())
            {
                var response = await Leaderboard.GetOrCreate((ulong)type);
                if (response.result != Model.RequestResult.Success)
                    return null;
                leaderboard = response.item;
                leaderboards[type] = leaderboard;
            }
            return leaderboard;
        }

        private async Task ProcessDeath(Character death)
        {
            var value = death.deathValue;
            if (!await CheckAgainstLeaderboard(LeaderboardType.AllTime, death, value) ||
                !await CheckAgainstLeaderboard(LeaderboardType.Monthly, death, value) ||
                !await CheckAgainstLeaderboard(LeaderboardType.Weekly, death, value))
            {
                deathQueue.Push(death);
            }
        }

        private async Task ProcessLiving(Character living)
        {
            ulong value = 0;
            if (living.statistics.TryGetValue(CharacterStatisticType.SoulsEarned, out var stat))
            {
                value = stat.value;
            }

            if (living.dead)
            {
                if (!await RemoveFromLeaderboard(LeaderboardType.Living, living))
                {
                    livingQueue.Push(living);
                }
            }
            else
            {
                if (!await CheckAgainstLeaderboard(LeaderboardType.Living, living, value))
                {
                    livingQueue.Push(living);
                }
            }
        }

        private async Task<bool> RemoveFromLeaderboard(LeaderboardType type, Character character)
        {
            var leaderboard = await GetLeaderboard(type);
            if (leaderboard == null)
            {
                Log.Write($"Failed to get Leaderboard: {type}");
                return false;
            }

            if (!await leaderboard.RemoveFromLeaderboard(character.id))
            {
                Log.Write($"Failed to remove from leaderboard ({type}), reloading...");
                return false;
            }
            return true;
        }

        private async Task<bool> CheckAgainstLeaderboard(LeaderboardType type, Character character, ulong value)
        {
            var leaderboard = await GetLeaderboard(type);
            if (leaderboard == null)
            {
                Log.Write($"Failed to get Leaderboard: {type}");
                return false;
            }

            if (leaderboard.characterIds.Count == 0)
            {
                if (!await leaderboard.Insert(0, character, value))
                {
                    Log.Write($"Failed to insert into leaderboard ({type}), reloading...");
                    return false;
                }
                return true;
            }

            leaderboard.Remove(character.id);
            for (int i = leaderboard.values.Count - 1; i >= 0; i--)
            {
                var leaderboardValue = leaderboard.values[i];
                if (value < leaderboardValue)
                {
                    if (i + 1 >= Leaderboard.Max_Leaderboard_Count) return true;
                    if (!await leaderboard.Insert(i + 1, character, value))
                    {
                        Log.Write($"Failed to insert into leaderboard ({type}), reloading...");
                        return false;
                    }
                    return true;
                }
            }

            if (!await leaderboard.Insert(0, character, value))
            {
                Log.Write($"Failed to insert into leaderboard ({type}), reloading...");
                return false;
            }

            return true;
        }
    }
}
