namespace BullOak.Repositories
{
    using BullOak.Repositories.EventPublisher;

    public interface IConfigureEventPublisher
    {
        IManuallyConfigureEventAppliers WithEventPublisher(IPublishEvents EventPublisher);
    }
}