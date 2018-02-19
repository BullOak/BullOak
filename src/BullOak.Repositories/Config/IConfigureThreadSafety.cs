namespace BullOak.Repositories
{
    using System;

    public interface IConfigureThreadSafety : IConfigureBullOak
    {
        IConfigureEventPublisher WithThreadSafetySelector(Func<Type, bool> threadSafetySelector);
    }
}