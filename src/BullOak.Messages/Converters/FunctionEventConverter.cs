namespace BullOak.Messages.Converters
{
    using System;

    public class FunctionEventConverter<TSource, TDestination> : EventConverterBase<TSource, TDestination>
        where TSource : IParcelVisionEvent
        where TDestination : IParcelVisionEvent
    {
        private readonly Func<TSource, TDestination> converter;

        public FunctionEventConverter(Func<TSource, TDestination> converter)
        {
            if (converter == null) throw new ArgumentNullException(nameof(converter));

            this.converter = converter;
        }

        public override TDestination Convert(TSource @event) => converter(@event);

        public static implicit operator FunctionEventConverter<TSource, TDestination>(Func<TSource, TDestination> converterFunction)
        {
            return new FunctionEventConverter<TSource, TDestination>(converterFunction);
        }
    }
}
