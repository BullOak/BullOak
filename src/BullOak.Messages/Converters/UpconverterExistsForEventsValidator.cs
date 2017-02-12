namespace BullOak.Messages.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public static class UpconverterExistsForEventsValidator
    {
        private static readonly Type parcelVisionEventInterfaceType = typeof(IParcelVisionEvent);


        public static void DiscoverEventsAndValidateUpconvertersExist(this IEnumerable<Assembly> assembliesContainingEvents,
            IEnumerable<IEventConverter> converters)
        {
            assembliesContainingEvents.DiscoverEventsAndValidateUpconvertersExist(t => t.Namespace.Contains("V1"), converters);
        }

        public static void DiscoverEventsAndValidateUpconvertersExist(this IEnumerable<Assembly> assembliesContainingEvents,
            Func<Type, bool> isOriginEventPredicate, IEnumerable<IEventConverter> converters)
        {
            CheckIfUpconvertersExistOrThrow(assembliesContainingEvents.SelectMany(x=> x.GetTypes())
                .Where(x=> parcelVisionEventInterfaceType.IsAssignableFrom(x)), isOriginEventPredicate, converters);
        }

        public static void CheckIfUpconvertersExistOrThrow(IEnumerable<Type> eventTypes,
            IEnumerable<IEventConverter> converters)
        {
            CheckIfUpconvertersExistOrThrow(eventTypes, t => t.Namespace.Contains(".V1"), converters);
        }

        public static void CheckIfUpconvertersExistOrThrow(IEnumerable<Type> eventTypes, Func<Type, bool> isOriginEventChecker,
            IEnumerable<IEventConverter> converters)
        {
            var eventTypesMissingAnUpconverter = new List<Type>();
            var converterTargets = new HashSet<Type>();

            foreach (var eventConverter in converters)
            {
                converterTargets.Add(eventConverter.DestinationType);
            }

            foreach (var eventType in eventTypes)
            {
                if (!parcelVisionEventInterfaceType.IsAssignableFrom(eventType))
                {
                    throw new ArgumentException($"{eventType.FullName} is not a ParcelVision event.");
                }

                if (!isOriginEventChecker(eventType) && !converterTargets.Contains(eventType))
                {
                    eventTypesMissingAnUpconverter.Add(eventType);
                }
            }

            if (eventTypesMissingAnUpconverter.Count > 0)
            {
                throw new MissingUpconverterException(eventTypesMissingAnUpconverter.Select(x=> x.FullName));
            }
        }
    }
}
