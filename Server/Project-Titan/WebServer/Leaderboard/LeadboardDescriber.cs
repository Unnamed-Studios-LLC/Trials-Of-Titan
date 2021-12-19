using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TitanCore.Core;
using TitanCore.Net.Web;
using TitanDatabase;
using TitanDatabase.Leaderboards;

namespace WebServer.Leaderboard
{
    public class LeaderboardDescriber
    {
        private class LeaderboardDescription
        {
            public LeaderboardType type;

            public WebLeaderboardInfo[] infos;

            public DateTime lastUpdated = DateTime.Now;

            public int updating = 0;

            public LeaderboardDescription(LeaderboardType type, WebLeaderboardInfo[] infos)
            {
                this.type = type;
                this.infos = infos;
            }

            public void CheckUpdate()
            {
                if (updating == 0 && (DateTime.Now - lastUpdated).TotalMinutes > 5)
                {
                    Update();
                }
            }

            private async void Update()
            {
                if (Interlocked.CompareExchange(ref updating, 1, 0) != 0) return;

                var leaderboard = await LeaderboardManager.Get(type);
                var infos = await Database.DescribeLeaderboard(leaderboard);
                this.infos = infos.ToArray();

                lastUpdated = DateTime.Now;
                updating = 0;
            }
        }

        public static async Task<LeaderboardDescriber> Load()
        {
            var types = (LeaderboardType[])Enum.GetValues(typeof(LeaderboardType));
            var descriptions = new List<LeaderboardDescription>();
            foreach (var type in types)
            {
                var leaderboard = await LeaderboardManager.Get(type);
                var infos = await Database.DescribeLeaderboard(leaderboard);
                descriptions.Add(new LeaderboardDescription(type, infos.ToArray()));
            }

            return new LeaderboardDescriber(descriptions.ToArray());
        }

        private ConcurrentDictionary<LeaderboardType, LeaderboardDescription> leaderboards = new ConcurrentDictionary<LeaderboardType, LeaderboardDescription>();

        private LeaderboardDescriber(LeaderboardDescription[] leaderboards)
        {
            foreach (var leaderboard in leaderboards)
                this.leaderboards[leaderboard.type] = leaderboard;
        }

        public WebLeaderboardInfo[] GetLeaderboard(LeaderboardType type)
        {
            if (!leaderboards.TryGetValue(type, out var description))
                return null;

            description.CheckUpdate();
            return description.infos;
        }
    }
}
