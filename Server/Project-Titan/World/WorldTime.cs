using System;

namespace World
{
    public struct WorldTime
    {
        /// <summary>
        /// Id of the current tick
        /// </summary>
        public ulong tickId;
        
        /// <summary>
        /// Time since the start of the world
        /// </summary>
        public double totalTime;

        /// <summary>
        /// The time since the last tick
        /// </summary>
        public double deltaTime;

        public WorldTime(ulong tickId, double totalTime, double deltaTime)
        {
            this.tickId = tickId;
            this.totalTime = totalTime;
            this.deltaTime = deltaTime;
        }
    }
}
