namespace BullOak.Repositories.Appliers
{
    using BullOak.Repositories;
    using System;

    public struct StoredEvent
    {
        public readonly Type EventType;
        public readonly object Event;
        public readonly long EventIndex;

        public StoredEvent(Type eventType, object @event, long eventIndex)
        {
            EventType = eventType;
            Event = @event;
            EventIndex = eventIndex;
        }

        public ItemWithType ToItemWithType()
            => (ItemWithType)this;

        public static implicit operator ItemWithType(StoredEvent se)
            => new ItemWithType(se.Event, se.EventType);

        public static StoredEvent FromItemWithType(ItemWithType item, long eventIndex)
            => new StoredEvent(item.type, item.instance, eventIndex);
    }
}
