namespace BullOak.Messages.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ConvertFunction = System.Func<IParcelVisionEvent, System.Collections.Generic.IEnumerable<IParcelVisionEvent>>;
    using ConverterTree = System.Collections.Generic.Dictionary<System.Type, System.Func<IParcelVisionEvent, System.Collections.Generic.IEnumerable<IParcelVisionEvent>>>;
    using ConverterList = System.Collections.Generic.IEnumerable<ConverterTuple>;

    public class RecursiveEventUpconverter
    {
        private readonly ConverterTree converters;

        public RecursiveEventUpconverter(params IEventConverter[] eventConverters)
            :this((IEnumerable<IEventConverter>)eventConverters)
        { }

        public RecursiveEventUpconverter(IEnumerable<IEventConverter> eventConverters)
        {
            eventConverters = DeduplicateConverters(eventConverters);

            var converterList = (eventConverters ?? Enumerable.Empty<IEventConverter>())
                .Select(c => c.SourceType)
                .Distinct()
                .SelectMany(t => GetConvertersFor(t, eventConverters));

            converterList = RemoveDuplicatePathsByChosingShorter(converterList);

            converters = converterList.GroupBy(x => x.SourceType)
                .ToDictionary(x => x.Key, x => (ConvertFunction)(e => x.Select(c => c.ConvertFunction(e))));
        }

        private static ConverterList RemoveDuplicatePathsByChosingShorter(ConverterList converters)
        {
            foreach (var converterGroup in converters.GroupBy(x=> $"{x.SourceType.FullName}{x.DestinationType.FullName}"))
            {
                yield return converterGroup.OrderBy(x => x.ConverterCost).FirstOrDefault();
            }
        }

        private class ConverterEqualityComparer : IEqualityComparer<IEventConverter>
        {
            private string GetDescriptor(IEventConverter converter)
            {
                return converter?.SourceType.FullName + converter?.DestinationType.FullName;
            }
            public bool Equals(IEventConverter x, IEventConverter y)
            {
                return GetDescriptor(x).Equals(GetDescriptor(y));
            }

            public int GetHashCode(IEventConverter converter)
            {
                return GetDescriptor(converter).GetHashCode();
            }
        }
        private static IEnumerable<IEventConverter> DeduplicateConverters(IEnumerable<IEventConverter> converters)
        {
            return converters.Distinct(new ConverterEqualityComparer());
        }

        private static ConverterList GetConvertersFor(
            Type sourceType,
            IEnumerable<IEventConverter> allConverters)
        {
            //DANGER: Recursion conbined with Linq. Do not touch unless you know what you are doing.
            // to be clear, this is not that performant, but it is only running once on load to set up the
            // converter tree.
            return allConverters.Where(c => c.SourceType == sourceType)
                .SelectMany(sourceConverter => GetConvertersFor(sourceConverter.DestinationType, allConverters)
                    .Select(innerConverter => new ConverterTuple(sourceType, innerConverter.DestinationType, innerConverter.ConverterCost + 1, @event => innerConverter.ConvertFunction(sourceConverter.Convert(@event))))
                    .DefaultIfEmpty(new ConverterTuple(sourceType, sourceConverter.DestinationType, 1, sourceConverter.Convert)));
        }

        public IEnumerable<IParcelVisionEvent> UpconvertEvent(IParcelVisionEvent originalEvent)
        {
            ConvertFunction converterFunction;

            if (converters.TryGetValue(originalEvent.GetType(), out converterFunction))
            {
                foreach (var convertedEvent in converterFunction(originalEvent))
                {
                    yield return convertedEvent;
                }
            }
            else
            {
                yield return originalEvent;
            }
        }
    }
}
