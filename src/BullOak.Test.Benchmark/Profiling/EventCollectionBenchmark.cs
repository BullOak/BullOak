namespace BullOak.Test.Benchmark.Profiling
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Attributes.Jobs;
    using BullOak.Repositories.Session;
    using BullOak.Repositories.Session.CustomLinkedList;

    [ShortRunJob]
    //The results show that for thread safe operations, the custom collection is between 2-5 times faster
    //For unsafe operations, this custom collection is more performant until up to 12 events (roughly)
    //   For event count larger than 12, a generic list performs better.
    public class EventCollectionBenchmark
    {
        [Params(1, 3, 10)]
        public int NumberOfEvents { get; set; }

        public object[] defaultEvents;

        [IterationSetup]
        public void Setup()
        {
            defaultEvents = new object[NumberOfEvents];
            for (int i = 0; i < defaultEvents.Length; i++)
                defaultEvents[i] = new object();
        }

        [Benchmark]
        public object[] MyCollectionAddEventsWithSpinlockAndGetBuffer()
        {
            var sut = new Repositories.Session.CustomLinkedList.LinkedList<object>();

            for (int i = 0; i < NumberOfEvents; i++)
                sut.Add(defaultEvents[i]);

            return sut.GetBuffer();
        }

        //[Benchmark]
        //public object[] MyCollectionAddEventsUnsafeAndGetBuffer()
        //{
        //    var sut = new NodeCollection<object>();

        //    for (int i = 0; i < NumberOfEvents; i++)
        //        sut.AddUnsafe(defaultEvents[i]);

        //    return sut.GetBuffer();
        //}

        //[Benchmark]
        //public object[] LinkedListAddEventsAndGetBuffer()
        //{
        //    var sut = new LinkedList<object>();

        //    for (int i = 0; i < NumberOfEvents; i++)
        //        sut.AddLast(defaultEvents[i]);

        //    return sut.ToArray();
        //}

        //[Benchmark]
        //public object[] ListAddEventAndGetBuffer()
        //{
        //    var sut = new List<object>();

        //    for (int i = 0; i < NumberOfEvents; i++)
        //        sut.Add(defaultEvents[i]);

        //    return sut.ToArray();
        //}

        //[Benchmark]
        //public object[] ConcurrentQueueAddEventAndGetBuffer()
        //{
        //    var sut = new ConcurrentQueue<object>();

        //    for (int i = 0; i < NumberOfEvents; i++)
        //        sut.Enqueue(defaultEvents[i]);

        //    return sut.ToArray();
        //}
    }
}