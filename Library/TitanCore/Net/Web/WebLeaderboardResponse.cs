using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;

namespace TitanCore.Net.Web
{
    public enum WebLeaderboardResult
    {
        Success,
        InvalidRequest,
        InternalServerError,
        RateLimitReached
    }

    public class WebLeaderboardResponse
    {
        public WebLeaderboardResult result;

        public WebLeaderboardInfo[] leaderboard;

        public WebLeaderboardResponse()
        {

        }

        public WebLeaderboardResponse(WebLeaderboardResult result)
        {
            this.result = result;
            this.leaderboard = null;
        }

        public WebLeaderboardResponse(WebLeaderboardResult result, WebLeaderboardInfo[] leaderboard)
        {
            this.result = result;
            this.leaderboard = leaderboard;
        }
    }

    public class WebLeaderboardInfo
    {
        public string name;

        public ushort type;

        public ushort skin;

        public Item[] equips;

        public ulong value;

        public CharacterStatistic[] statistics;

        public WebLeaderboardInfo()
        {

        }

        public WebLeaderboardInfo(string name, ushort type, ushort skin, Item[] equips, ulong value, CharacterStatistic[] statistics)
        {
            this.name = name;
            this.type = type;
            this.skin = skin;
            this.equips = equips;
            this.value = value;
            this.statistics = statistics;
        }
    }
}
