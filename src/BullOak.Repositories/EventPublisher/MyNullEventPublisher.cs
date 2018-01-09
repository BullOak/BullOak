namespace BullOak.Repositories.EventPublisher
{
    using System.Threading;
    using System.Threading.Tasks;

    internal class MyNullEventPublisher : IPublishEvents
    {
        private static readonly Task Done = Task.FromResult(0);
        public static readonly MyNullEventPublisher Instance = new MyNullEventPublisher();

        public Task Publish(object @event, CancellationToken? cancellationToken = null) => Done;

        public void PublishSync(object @event)
        { }


        private MyNullEventPublisher()
        {
        }
    }
}
