namespace BullOak.Repositories.EventPublisher
{
    using System.Threading;
    using System.Threading.Tasks;

    internal class MyNullEventPublisher : IPublishEvents
    {
        private static readonly Task Done = Task.FromResult(0);
        public static readonly MyNullEventPublisher Instance = new MyNullEventPublisher();

        public Task Publish(ItemWithType @event, CancellationToken cancellationToken = default(CancellationToken)) => Done;

        public void PublishSync(ItemWithType @event)
        { }


        private MyNullEventPublisher()
        {
        }
    }
}
