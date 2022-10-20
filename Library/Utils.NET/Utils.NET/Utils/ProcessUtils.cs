using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Utils.NET.Utils
{
    public static class ProcessUtils
    {
        public static void WaitForProcessToClose(int processId, double timeoutAfter, bool killRetry)
        {
            var process = Process.GetProcessById(processId);
            if (process == null) return;

            int ms = (int)(timeoutAfter * 1000);

            process.WaitForExit(ms);

            if (killRetry)
            {
                process.Kill();
                process.WaitForExit(ms);
            }
        }
    }
}
