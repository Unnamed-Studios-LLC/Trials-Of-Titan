using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.Collections;
using Utils.NET.Logging;

namespace World.Net
{
    public class SpamChecker
    {
        private int eventsPerWindow;

        private float eventWindow;

        private Range cooldownBounds;

        private float cooldownGrowth;

        private float cooldown;

        private DateTime eventWindowTime;

        private int eventCount;

        public SpamChecker(int eventsPerWindow, float eventWindow, Range cooldownBounds, float cooldownGrowth)
        {
            this.eventsPerWindow = eventsPerWindow;
            this.eventWindow = eventWindow;
            this.cooldownBounds = cooldownBounds;
            this.cooldownGrowth = cooldownGrowth;

            cooldown = cooldownBounds.min;
            eventWindowTime = DateTime.Now;
        }

        public bool Event()
        {
            var now = DateTime.Now;
            if (InCooldown(now)) return false;

            ValidEvent(now);

            return !InCooldown(now);
        }

        private bool InCooldown(DateTime now)
        {
            return now < eventWindowTime;
        }

        private void ValidEvent(DateTime now)
        {
            var dif = (now - eventWindowTime).TotalSeconds; // if this event is past the previous window, start a new window
            if (dif > eventWindow)
            {
                var windowCount = (int)(dif / eventWindow);
                cooldown = Math.Max(cooldownBounds.min, cooldown - cooldownGrowth * windowCount);
                eventWindowTime = now;
                eventCount = 0;
            }

            eventCount++;

            if (eventCount > eventsPerWindow) // if this event goes above the events per window, trigger a cooldown
            {
                eventWindowTime = now.AddSeconds(cooldown);
                eventCount = 0;
                cooldown = Math.Min(cooldownBounds.max, cooldown + cooldownGrowth);

            }
        }
    }
}
