namespace BullOak.Repositories
{
    using BullOak.Repositories.StateEmit;

    public interface IConfigureStateFactory
    {
        IConfigureThreadSafety WithStateFactory(ICreateStateInstances stateFactory);
    }
}