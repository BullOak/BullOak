namespace BullOak.Messages.Converters
{
    using System;

    public abstract class EventConverterBase<TSource, TDestination> : IEventConverter
        where TSource : IParcelVisionEvent
        where TDestination : IParcelVisionEvent
    {
        private readonly Lazy<Type> sourceType = new Lazy<Type>(() => typeof(TSource));
        public Type SourceType => sourceType.Value;
        private readonly Lazy<Type> destinationType = new Lazy<Type>(() => typeof(TDestination));
        public Type DestinationType => destinationType.Value;

        bool IEventConverter.CanConvert(IParcelVisionEvent @event)
        {
            return @event.GetType().IsAssignableFrom(SourceType);
        }

        IParcelVisionEvent IEventConverter.Convert(IParcelVisionEvent @event)
        {
            if (((IEventConverter) this).CanConvert(@event))
            {
                return Convert((TSource) @event);
            }

            throw new ArgumentException(nameof(@event));
        }

        public abstract TDestination Convert(TSource @event);
    }
}
