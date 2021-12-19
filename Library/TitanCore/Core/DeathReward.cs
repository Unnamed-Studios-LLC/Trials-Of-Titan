using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TitanCore.Core
{
    public enum DeathRewardType
    {

    }

    public class DeathReward
    {
        public long totalReward;

        public Dictionary<DeathRewardType, long> rewards;

        public DeathReward(long baseReward, CharacterStatistic[] statistics)
        {
            var statDict = statistics.ToDictionary(_ => _.type);
            var rewards = new Dictionary<DeathRewardType, long>();
            long rewardExtra = 0;

            totalReward = baseReward + rewardExtra;
        }
    }
}
