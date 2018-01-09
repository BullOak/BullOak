namespace BullOak.Repositories.EventPublisher
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    internal class MySyncEventPublisher : IPublishEvents
    {
        private static readonly Task Done = Task.FromResult(0);
        public readonly Action<object> publish;

        public MySyncEventPublisher(Action<object> publish)
            => this.publish = publish ?? throw new ArgumentNullException(nameof(publish));

        public Task Publish(object @event, CancellationToken? cancellationToken = null)
        {
            PublishSync(@event);
            return Done;
        }

        public void PublishSync(object @event) => publish(@event);
    }
}