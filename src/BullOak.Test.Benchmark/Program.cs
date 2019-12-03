namespace BullOak.Test.Benchmark
{
    using BenchmarkDotNet.Running;

    class Program
    {
        static void Main() =>
            BenchmarkSwitcher
                .FromAssembly(typeof(Program).Assembly)
                .RunAll();
    }
}
