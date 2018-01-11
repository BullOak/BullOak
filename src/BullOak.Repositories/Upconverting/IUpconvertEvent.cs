namespace BullOak.Repositories.Upconverting
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.Serialization;

    public interface IUpconvertStoredItems
    {
        object Upconvert(object eventOrMemento, Type eventOrMementoType);
    }

    public interface IUpconvertEvent<TSource, TEvent>
    {
        TEvent Upconvert(TSource source);
    }

    internal class EventUpconverter : IUpconvertStoredItems
    {
        private readonly Dictionary<Type, Func<object, object>> upconverters;

        internal EventUpconverter(Dictionary<Type, Func<object, object>> upconverters)
            => this.upconverters = upconverters ?? throw new ArgumentNullException(nameof(upconverters));

        public object Upconvert(object eventOrMemento, Type eventOrMementoType)
            => upconverters.TryGetValue(eventOrMementoType, out var upconverter)
                ? upconverter(eventOrMemento)
                : eventOrMemento;
    }

    internal class JoinedUpconverter<TSource, TIntermediate, TDestination> : IUpconvertEvent<TSource, TDestination>
    {
        public readonly IUpconvertEvent<TSource, TIntermediate> first;
        public readonly IUpconvertEvent<TIntermediate, TDestination> last;

        public JoinedUpconverter(IUpconvertEvent<TSource, TIntermediate> first,
            IUpconvertEvent<TIntermediate, TDestination> last)
        {
            this.first = first;
            this.last = last;
        }

        public TDestination Upconvert(TSource source)
            => last.Upconvert(first.Upconvert(source));
    }

    internal class UpconverterProcessor
    {
        private class UpconverterInfo
        {
            public readonly Type upconverterType;
            public readonly Type sourceType;
            public readonly Type destinationType;
            public readonly object upconverterInstance;

            public UpconverterInfo(Type upconverterType,
                object upconverterInstance,
                Type sourceType,
                Type destinationType)
            {
                this.upconverterType = upconverterType;
                this.upconverterInstance = upconverterInstance;
                this.sourceType = sourceType;
                this.destinationType = destinationType;
            }
        }

        public Dictionary<Type, Func<object, object>> DiscoverAndCreateUpconverters(IEnumerable<Type> loadedTypes)
        {
            var openUpconverterType = typeof(IUpconvertEvent<,>);

            var upconverterTypesLoaded = loadedTypes
                .Where(x => x.GetGenericTypeDefinition() == openUpconverterType);

            var upconverterInfo = new List<UpconverterInfo>();

            foreach (var upconverterType in upconverterTypesLoaded)
            {
                try
                {
                    var convertingTypes = upconverterType.GetGenericArguments();

                    var instance = Activator.CreateInstance(upconverterType);

                    upconverterInfo.Add(new UpconverterInfo(upconverterType, instance, convertingTypes[0],
                        convertingTypes[1]));
                }
                catch (Exception ex)
                {
                    throw new CannotInstantiateConverterException(upconverterType);
                }
            }

            return upconverterInfo
                .Select(x => new {x.sourceType, UpconvertFunction = GetChainCalls(x, upconverterInfo)})
                .ToDictionary(x => x.sourceType, x => x.UpconvertFunction);
        }

        private Func<object, object> GetChainCalls(UpconverterInfo upconverter, List<UpconverterInfo> upconverterInfo)
        {
        }

        private UpconverterInfo FindNext(Type sourceType, List<UpconverterInfo> upconverterInfo)
            => upconverterInfo.SingleOrDefault(x => x.sourceType == sourceType);

        private static IUpconvertEvent<TSource, TDestination> Join<TSource, TIntermediate, TDestination>(
            IUpconvertEvent<TSource, TIntermediate> left,
            IUpconvertEvent<TIntermediate, TDestination> right)
            => new JoinedUpconverter<TSource, TIntermediate, TDestination>(left, right);


    }

    [Serializable]
    internal class CannotInstantiateConverterException : Exception
    {
        private Type upconverterType;


        public CannotInstantiateConverterException(Type upconverterType)
        {
            this.upconverterType = upconverterType;
        }

        public CannotInstantiateConverterException(Type upconverterType, Exception innerException)
            : base(String.Empty, innerException)
            => this.upconverterType = upconverterType;
    }
}
