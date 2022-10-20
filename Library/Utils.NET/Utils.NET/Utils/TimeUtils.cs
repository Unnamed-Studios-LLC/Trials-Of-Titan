using System;
using System.Collections.Generic;
using System.Text;

namespace Utils.NET.Utils
{
    public static class TimeUtils
    {
        public static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static ulong ToEpochSeconds(DateTime time)
        {
            time = time.ToUniversalTime();
            return Convert.ToUInt64((time - epoch).TotalSeconds);
        }

        public static DateTime FromEpochSeconds(ulong seconds)
        {
            return epoch.AddSeconds(seconds);
        }
    }
}
