using System;
using System.Collections.Generic;
using System.Text;

namespace TitanCore.Core
{
    public struct StatusEffectTime
    {
        public uint startTime;

        public uint endTime;

        public StatusEffectTime(uint startTime, uint endTime)
        {
            this.startTime = startTime;
            this.endTime = endTime;
        }

        public bool HasEffect(uint time)
        {
            return time > startTime && time < endTime;
        }
    }
}
