namespace BullOak.Repositories.Upconverting
{
    using System.Collections.Generic;

    public interface IUpconvertEvent<TSource>
    {
        IEnumerable<object> Upconvert(TSource source);
    }

    public interface IUpconvertEvent<TSource, TDestination>
    {
        TDestination Upconvert(TSource source);
    }
}
