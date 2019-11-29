namespace BullOak.Test.Benchmark.Behavioural
{
    using BenchmarkDotNet.Attributes;

    public class CodeTests
    {
        public class ToTest
        {
            public readonly int myReadonly;
            public int myEditable;

            public ToTest(int toEditable)
            {
                myEditable = toEditable;
            }

            public ToTest(int toReadonly, bool t)
            {
                myReadonly = toReadonly;
            }
        }

        [GlobalSetup]
        public void Setup()
        {
        }

        [Benchmark]
        public int ToReadonly()
        {
            return new ToTest(2,true).myReadonly;
        }

        [Benchmark]
        public int UsingCoalecenseInBody()
        {
            return new ToTest(2).myEditable;
        }

        //[Benchmark]
        //public int UsingCoalecenseInExpr()
        //    => shouldPass ? one : two;
    }
}
