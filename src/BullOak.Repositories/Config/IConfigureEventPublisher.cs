namespace BullOak.Repositories
{
    using EventPublisher = System.Func<object, System.Threading.Tasks.Task>;

    public interface IConfigureEventPublisher
    {
        IManuallyConfigureEventAppliers WithEventPublisher(EventPublisher publish);
    }
}