using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Utils.NET.Utils
{
    public static class ConvUtils
    {
        /// <summary>
        /// Retries a task until success or a max retry count is reached
        /// </summary>
        /// <param name="task"></param>
        /// <param name="maxRetries"></param>
        /// <returns></returns>
        public static async Task<bool> RetryUntil(Task<bool> task, int maxRetries)
        {
            int tryCount = 0;
            while (tryCount < maxRetries && !await task)
            {
                tryCount++;
            }
            return tryCount < maxRetries;
        }
    }
}
