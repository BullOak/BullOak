namespace BullOak.Infrastructure.TestHelpers.Application.xUnit
{
    using System;
    using System.Diagnostics;

    internal class PerformanceProfilingTracer
    {
        private string contextName;
        private long stopwatchAtStart;
        private long allocatedBytesAtStart;
        private int gc0CountAtStart;
        private int gc1CountAtStart;
        private int gc2CountAtStart;
        private ulong cpuCyclesAtStart;

        public PerformanceProfilingTracer(string contextName)
        {
            this.contextName = contextName;
        }

        public void Start()
        {
            allocatedBytesAtStart = GC.GetTotalMemory(true);
            gc0CountAtStart = GC.CollectionCount(0);
            gc1CountAtStart = GC.CollectionCount(1);
            gc2CountAtStart = GC.CollectionCount(2);
            stopwatchAtStart = Stopwatch.GetTimestamp();
            cpuCyclesAtStart = CpuProfiler.GetCurrentProcessCpuCycles();
        }

        public PerformanceSummary Stop()
        {
            var elapsedTicks = Stopwatch.GetTimestamp() - stopwatchAtStart;
            var elapsedMilliseconds = (elapsedTicks * 1000) / Stopwatch.Frequency;
            var cpuCycles = CpuProfiler.GetCurrentProcessCpuCycles() - cpuCyclesAtStart;
            var allocatedBytes = GC.GetTotalMemory(true) - allocatedBytesAtStart;
            var gc0Count = GC.CollectionCount(0) - gc0CountAtStart;
            var gc1Count = GC.CollectionCount(1) - gc1CountAtStart;
            var gc2Count = GC.CollectionCount(2) - gc2CountAtStart;

            return new PerformanceSummary(
                this.contextName,
                elapsedMilliseconds,
                cpuCycles,
                allocatedBytes,
                gc0Count,
                gc1Count,
                gc2Count);
        }
    }
}
