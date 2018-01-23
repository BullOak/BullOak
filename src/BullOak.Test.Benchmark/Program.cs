namespace BullOak.Test.Benchmark
{
    using BenchmarkDotNet.Running;
    using BullOak.Test.Benchmark.Behavioural;
    using System;
    using System.Collections.Generic;

    class Program
    {
        static void Main(string[] args)
        {
            //RunForVSProfiling<EditChildEntitiesBenchmark>(b => b.EditChildFromRepoBasedAggregate(),
            //    b =>
            //    {
            //        b.Capacity = 100;
            //        b.SeatsToReserve = 20;
            //    });
            //Console.ReadKey();

            //ProfileEditRepoBasedChilds();
            RunBenchmark();
        }

        private static void RunForVSProfiling<TBenchmark>(Action<TBenchmark> methodToCall,
            Action<TBenchmark> setupParameters)
        {
            var instance = Activator.CreateInstance<TBenchmark>();

            //Set any parameters
            setupParameters(instance);

            //Call setup methods
            ((dynamic)instance).Setup();

            //Loop to infinity for profiler.
            var endOn = DateTime.UtcNow + TimeSpan.FromSeconds(30);
            while (endOn > DateTime.UtcNow)
            {
                for (int i = 0; i < 5000; i++)
                    methodToCall(instance);
            }
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


            //benchmarks.AddRange(BenchmarkConverter.TypeToBenchmarks(typeof(CodeTests)));

            //benchmarks.AddRange(BenchmarkConverter.TypeToBenchmarks(typeof(SaveAggregateBenchmark)));
            //benchmarks.AddRange(BenchmarkConverter.TypeToBenchmarks(typeof(LoadAggregateWithChildEntitiesBenchmark)));
            //benchmarks.AddRange(BenchmarkConverter.TypeToBenchmarks(typeof(EditChildEntitiesBenchmark)));
            //benchmarks.AddRange(BenchmarkConverter.TypeToBenchmarks(typeof(RepoBasedWithVariableReconstitutors)));

            //benchmarks.AddRange(BenchmarkConverter.TypeToBenchmarks(typeof(LoadAggregateOneEventNoChildsBenchmark)));
            //benchmarks.AddRange(BenchmarkConverter.TypeToBenchmarks(typeof(SaveChildEntityBenchmark)));
            //benchmarks.AddRange(BenchmarkConverter.TypeToBenchmarks(typeof(EventCollectionBenchmark)));

            var summary = BenchmarkRunner.Run(benchmarks.ToArray(), null);
        }
    }
}
