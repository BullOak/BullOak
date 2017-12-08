namespace BullOak.Test.Benchmark
{
    using System;
    using System.Collections.Generic;
    using BenchmarkDotNet.Running;
    using BullOak.Test.Benchmark.Behavioural;
    using BullOak.Test.Benchmark.Profiling;

    class Program
    {
        static void Main(string[] args)
        {
            //ProfileEditRepoBasedChilds();
            RunBenchmark();
        }

        private static void ProfileEditRepoBasedChilds()
        {
            RunProfilerOn(new EditChildEntitiesBenchmark(), c =>
            {
                c.Capacity = 100;
                c.SeatsToReserve = 100;
                c.Setup();
            }, c => c.EditChildFromRepoBasedAggregate(), 1 * 1024 * 1024);
        }

        private static void RunProfilerOn<TClass>(Action<TClass> setup, Action<TClass> methodToProfile, int methodCallCount = 2000)
            where TClass : class, new()
            => RunProfilerOn(new TClass(), setup, methodToProfile, methodCallCount);

        private static void RunProfilerOn<TClass>(TClass testClass, Action<TClass> setup, Action<TClass> methodToProfile, int methodCallCount = 2000)
            where TClass : class
        {
            setup(testClass);

            for (int i = 0; i < methodCallCount; i++)
                methodToProfile(testClass);
        }

        private static void RunBenchmark()
        {
            var benchmarks = new List<Benchmark>();

            //benchmarks.AddRange(BenchmarkConverter.TypeToBenchmarks(typeof(SaveAggregateBenchmark)));
            //benchmarks.AddRange(BenchmarkConverter.TypeToBenchmarks(typeof(LoadAggregateOneEventNoChildsBenchmark)));
            //benchmarks.AddRange(BenchmarkConverter.TypeToBenchmarks(typeof(SaveChildEntityBenchmark)));
            //benchmarks.AddRange(BenchmarkConverter.TypeToBenchmarks(typeof(LoadAggregateWithChildEntitiesBenchmark)));
            benchmarks.AddRange(BenchmarkConverter.TypeToBenchmarks(typeof(EditChildEntitiesBenchmark)));
            //benchmarks.AddRange(BenchmarkConverter.TypeToBenchmarks(typeof(RepoBasedWithVariableReconstitutors)));
            //benchmarks.AddRange(BenchmarkConverter.TypeToBenchmarks(typeof(EventCollectionBenchmark)));

            var summary = BenchmarkRunner.Run(benchmarks.ToArray(), null);
        }
    }
}
