namespace BullOak.Messages.Converters
{
    using System;

    public interface IEventConverter
    {
        Type SourceType { get; }
        Type DestinationType { get; }

        bool CanConvert(IParcelVisionEvent @event);
        IParcelVisionEvent Convert(IParcelVisionEvent @event);
    }
}
