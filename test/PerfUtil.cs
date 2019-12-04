using NUnit.Framework;
using System;
using System.Diagnostics;

namespace SimEarthTests
{
    public class PerfUtil
    {
        private double benchmark;

        Stopwatch watch;

        public PerfUtil()
        {
            watch = new Stopwatch();
            SetUpBenchmark();
        }

        private void SetUpBenchmark()
        {
            benchmark = 0;
            long M = 100000;

            while (benchmark < 100)
            {
                M *= 10;
                GC.Collect();
                watch.Restart();
                for (long i = 0; i < M; i++)
                { }
                watch.Stop();
                benchmark = watch.ElapsedMilliseconds;
            }
            benchmark /= M;
            Assert.IsTrue(watch.ElapsedMilliseconds >= 100);
        }

        public double Profile(Action action)
        {
            GC.Collect();
            GC.WaitForFullGCComplete(-1);
            watch.Restart();
            action();
            watch.Stop();
            double k = watch.ElapsedMilliseconds / benchmark;
            return k;
        }

        public double GetEllapsedMilliseconds(double cycleCount)
        {
            return cycleCount * benchmark;
        }

        public double Profile(Action action, double expectedCycleCount)
        {
            double k = Profile(action);
            Assert.IsTrue(k < expectedCycleCount, "Operation took {0} cycles", k);
            return k;
        }
    }
}