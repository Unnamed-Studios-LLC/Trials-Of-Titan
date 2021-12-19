using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.Logging;

namespace World.Net
{
    public class TimeChecker
    {
        private uint lastClientTime;

        private DateTime lastServerTime;

        private double timeScale = 1;

        public void StartTime(uint time)
        {
            lastClientTime = time;
            lastServerTime = DateTime.Now;
            timeScale = 1;
        }

        public bool ValidTimeAdvance(uint time, double ping)
        {
            if (time < lastClientTime) return false;
            var delta = (time - lastClientTime);
            if (delta == 0)
            {
                return true;
            }
            var now = DateTime.Now;
            var realDelta = (now - lastServerTime).TotalMilliseconds + 1;
            delta += 1;

            var scale = realDelta / (double)delta;
            timeScale += (scale - timeScale) * 0.05f;
            var target = 0.8f - Math.Atan(ping / 80f) / 8f;

            if (timeScale < target) return false;

            lastClientTime = time;
            lastServerTime = now;
            return true;
        }
    }
}
