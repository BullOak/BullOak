namespace BullOak.Repositories.EventStore
{
    using System;

    public class EventMetadata_V1 : IHoldMetadata
    {
        public string EventTypeFQN { get; set; }
        public int MetadataVersion { get; set; }

        public EventMetadata_V1(string eventTypeFQN)
        {
            EventTypeFQN = eventTypeFQN ?? throw new ArgumentNullException(nameof(EventTypeFQN));
            MetadataVersion = 1;
        }

        internal static EventMetadata_V1 From(ItemWithType @event)
            => new EventMetadata_V1(@event.type.FullName);
    }
}