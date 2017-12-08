namespace BullOak.Repositories
{
    using System;

    public interface IConfigureThreadSafety
    {
        IConfigureEventPublisher WithThreadSafetySelector(Func<Type, bool> threadSafetySelector);
    }
}