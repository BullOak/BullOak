namespace BullOak.Repositories.Upconverting
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UpconvertFunc = System.Func<ItemWithType, UpconvertResult>;

    internal static class UpconverterCompiler
    {
        private static readonly Type openMultiEventUpconverterGeneric = typeof(IUpconvertEvent<>);
        private static readonly Type openSingleEventUpconverterGeneric = typeof(IUpconvertEvent<,>);

        public static Dictionary<Type, UpconvertFunc> GetFrom(IEnumerable<Type> discovered)
        {
            if(discovered == null) throw new ArgumentNullException(nameof(discovered));

            var filtered = discovered
                .Select(x=> new {ClassType = x, Interfaces = x.GetInterfaces()})
                .SelectMany(x=> x.Interfaces.Select(i => new {x.ClassType, Interface = i}))
                .Where(x => IsUpconvertInterface(x.Interface))
                .ToArray();

            if(filtered.Length <= 0) return new Dictionary<Type, UpconvertFunc>(0);

            var upconverters = new List<ItemWithType>();
            for (int i = 0; i < filtered.Length; i++)
            {
                var upconverterTypeInfo = filtered[i];
                try
                {
                    var upconverterInstance = new ItemWithType(Activator.CreateInstance(upconverterTypeInfo.ClassType), upconverterTypeInfo.Interface);
                    upconverters.Add(upconverterInstance);
                }
                catch(Exception ex)
                {
                    throw new CannotInstantiateUpconverterException(
                        $"Upconverter with the type {upconverterTypeInfo.ClassType.FullName} needs to have a public default ctor and be able to be instantiated using Activator.CreateInstance(Type)",
                        upconverterTypeInfo.ClassType, ex);
                }
            }

            return ToUpconverterFuncs(upconverters);
        }

        //private static IUpconvertStoredItems GetFromUpconverterInstances(IEnumerable<object> instances)
        //    => (EventUpconverter)(instance.ToUpconverterFuncs(instances));

        private static bool IsUpconvertInterface(Type x)
            => x.IsGenericType && (x.GetGenericTypeDefinition() == openSingleEventUpconverterGeneric || x.GetGenericTypeDefinition() == openMultiEventUpconverterGeneric);

        private static Dictionary<Type, UpconvertFunc> ToUpconverterFuncs(IEnumerable<ItemWithType> upconverters)
        {
            var singleEventUpconverters = upconverters
                .Where(x => x.type.GetGenericTypeDefinition() == openSingleEventUpconverterGeneric)
                .Select(x => new
                {
                    Instance = x.instance,
                    SourceEventType = x.type.GetGenericArguments()[0],
                    DestinationEventType = x.type.GetGenericArguments()[1]
                })
                .Select(x => 
                    (KeyValuePair<Type, UpconvertFunc>) ToFuncSingle(x.SourceEventType, x.DestinationEventType)
                    .Invoke(null, new[] {x.Instance}));

            var multiEventUpconverters = upconverters
                .Where(x => x.type.GetGenericTypeDefinition() == openMultiEventUpconverterGeneric)
                .Select(x => new
                {
                    Instance = x.instance,
                    SourceEventType = x.type.GetGenericArguments()[0]
                })
                .Select(x => (KeyValuePair<Type, UpconvertFunc>) ToFuncMultiple(x.SourceEventType)
                    .Invoke(null, new[] {x.Instance}));

            var allUpconverters = singleEventUpconverters.Concat(multiEventUpconverters)
                .ToList();

            var groupedBySourceEvent = allUpconverters.GroupBy(x => x.Key);

            if (groupedBySourceEvent.Any(x => x.Count() > 1))
            {
                throw new AggregateException(groupedBySourceEvent
                    .Where(x=> x.Count() > 1)
                    .Select(x=> new PreflightUpconverterConflictException(x.Key, x.First())));
            }

            return allUpconverters.ToDictionary(x => x.Key, x => x.Value);

            MethodInfo ToFuncMultiple(Type sourceEventType) => typeof(UpconverterCompiler)
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                .Where(x => x.Name == nameof(ToFuncFrom))
                .Single(x => x.ContainsGenericParameters && x.GetGenericArguments().Length == 1)
                .MakeGenericMethod(sourceEventType);

            MethodInfo ToFuncSingle(Type sourceEventType, Type destinationEventType) => typeof(UpconverterCompiler)
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                .Where(x => x.Name == nameof(ToFuncFrom))
                .Single(x => x.ContainsGenericParameters && x.GetGenericArguments().Length == 2)
                .MakeGenericMethod(sourceEventType, destinationEventType);
        }

        //Called through reflection
        private static KeyValuePair<Type, UpconvertFunc> ToFuncFrom<TSource>(IUpconvertEvent<TSource> upconverter)
            => new KeyValuePair<Type, UpconvertFunc>(typeof(TSource), x => new UpconvertResult(
                upconverter.Upconvert((TSource) x.instance).Select(i=> new ItemWithType(i))));

        //Called through reflection
        private static KeyValuePair<Type, UpconvertFunc> ToFuncFrom<TSource,TDestination>(IUpconvertEvent<TSource, TDestination> upconverter)
            => new KeyValuePair<Type, UpconvertFunc>(typeof(TSource), x => new UpconvertResult(new ItemWithType(
                upconverter.Upconvert((TSource)x.instance))));
    }
}
