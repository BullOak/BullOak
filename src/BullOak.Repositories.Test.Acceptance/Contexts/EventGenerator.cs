namespace BullOak.Repositories.Test.Acceptance.Contexts
{
    using System.Linq;

    internal class EventGenerator
    {
        public MyEvent[] GenerateEvents(int count)
            => Enumerable.Range(0, count).Select(x => new MyEvent(x)).ToArray();
    }
}
