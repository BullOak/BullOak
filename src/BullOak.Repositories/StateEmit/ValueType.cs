namespace BullOak.Repositories.StateEmit
{
    using System;
    using System.Linq.Expressions;

    public interface ValueType<T>
    {
        T With<TProp>(Expression<Func<T, TProp>> expression, TProp newValue);
    }
}
