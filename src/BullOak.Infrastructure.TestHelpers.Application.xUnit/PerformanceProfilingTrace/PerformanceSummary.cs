namespace BullOak.Infrastructure.TestHelpers.Application.xUnit
{
    internal struct PerformanceSummary
    {
        public string ContextName { get; private set; }
        public long ElapsedMilliseconds { get; private set; }
        public ulong CPUCycles { get; private set; }
        public long AllocatedBytes { get; private set; }
        public int GC0Count { get; private set; }
        public int GC1Count { get; private set; }
        public int GC2Count { get; private set; }

        public PerformanceSummary(
            string contextName,
            long elapsedMilliseconds,
            ulong cpuCycles,
            long allocatedBytes,
            int gcCount0,
            int gcCount1,
            int gcCount2)
        {
            this.ContextName = contextName;
            this.ElapsedMilliseconds = elapsedMilliseconds;
            this.CPUCycles = cpuCycles;
            this.AllocatedBytes = allocatedBytes;
            this.GC0Count = gcCount0;
            this.GC1Count = gcCount1;
            this.GC2Count = gcCount2;
        }

        public override string ToString()
        {
            return $"[UnitTestPerformanceTrace][{ContextName}] Elapsed: {ElapsedMilliseconds}ms; CPU Cycles: {CPUCycles}; Allocated {AllocatedBytes}bytes; GC0#: {GC0Count}; GC1#: {GC1Count}; GC2#: {GC2Count}";
        }
    }

}
