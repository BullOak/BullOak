// This is on purpose on a more basic\different namespace so as this extension method is available on the events in the tests
// ReSharper disable once CheckNamespace
namespace BullOak.Messages
{
    using System;
    using BullOak.Common;
    using BullOak.Infrastructure.TestHelpers.Application.Extensions;
    using Messages;

    public static class EventExtensions
    {
        public static IRequireOriginatingEntityType<TEvent, TSourceId> ToEnvelope<TEvent, TSourceId>(this TEvent @event, TSourceId sourceId)
            where TEvent : IParcelVisionEvent
            where TSourceId : IId, IEquatable<TSourceId>
        {
            //We are casting to dynamic so as to get the corret
            return ToDynamicEnvelope((dynamic)@event, (dynamic)sourceId);
        }

        private static IRequireOriginatingEntityType<TEvent, TSourceId> ToDynamicEnvelope<TEvent, TSourceId>(TEvent @event, TSourceId sourceId)
            where TEvent : IParcelVisionEvent
            where TSourceId : IId, IEquatable<TSourceId>
        {
            return new WithEventAndIdFactory<TEvent, TSourceId>(@event, sourceId);
        }
    }
}
