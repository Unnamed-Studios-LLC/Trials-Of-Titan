using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Utils.NET.Logging
{
    public class Benchmarker
    {
        private Dictionary<string, Action> tests = new Dictionary<string, Action>();

        public void AddTest(string name, Action method)
        {
            tests.Add(name, method);
        }

        public void RunTests()
        {
            var watch = new Stopwatch();
            var timings = new Dictionary<string, long>();
            foreach (var test in tests)
            {
                watch.Restart();
                test.Value.Invoke();
                watch.Stop();

                timings.Add(test.Key, watch.ElapsedTicks);
            }

            var timingList = timings.OrderBy(_ => _.Value);

            long first = -1;
            foreach (var timing in timingList)
            {
                var builder = new StringBuilder();
                if (timing.Value / TimeSpan.TicksPerMillisecond > 10)
                    builder.Append($"{timing.Key} time in MS: {(timing.Value / TimeSpan.TicksPerMillisecond)}");
                else
                    builder.Append($"{timing.Key} time in TICKS: {timing.Value}");

                if (first != -1)
                {
                    var dif = timing.Value - first;
                    builder.Append($" | {(int)((dif / (double)timing.Value) * 100)}% slower");
                }
                else
                {
                    first = timing.Value;
                }
                builder.Append('\n');
                Log.Write(builder.ToString());
            }
        }
    }
}
