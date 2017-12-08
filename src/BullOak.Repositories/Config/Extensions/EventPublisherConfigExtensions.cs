namespace BullOak.Repositories
{
    using System;
    using System.Threading.Tasks;

    public static class EventPublisherConfigExtensions
    {
        private static Task Done = Task.FromResult(0);

        public static IConfigureEventAppliers WithEventPublisher(
            this IConfigureEventPublisher eventPublisherConfig, Action<object> publish)
            => eventPublisherConfig.WithEventPublisher(o =>
            {
                publish(o);
                return Done;
            });

        public static IConfigureEventAppliers WithNoEventPublisher(
            this IConfigureEventPublisher eventPublisherConfig)
            => eventPublisherConfig.WithEventPublisher(_ => Done);
    }
}
