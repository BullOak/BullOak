namespace BullOak.Repositories
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using BullOak.Repositories.Appliers;

    internal class ApplierConfigurationBuilder : IManuallyConfigureEventAppliers
    {
        private readonly IConfigureEventAppliers baseConfiguration;
        private readonly BlockingCollection<ApplierRetriever> applierCollection;

        public ApplierConfigurationBuilder(IConfigureEventAppliers baseConfiguration)
        {
            this.baseConfiguration = baseConfiguration ?? throw new ArgumentNullException(nameof(baseConfiguration));

            applierCollection= new BlockingCollection<ApplierRetriever>();
        }

        public IConfigureUpconverter WithEventApplier(IApplyEventsToStates eventApplier)
            => applierCollection.Count == 0
                ? baseConfiguration.WithEventApplier(eventApplier)
                : throw new Exception(
                    $"Items already configured. Please either provide an instance of {nameof(IApplyEventsToStates)} or manually configure\\add each {typeof(IApplyEvents<>).Name}");

        private static readonly Type typeOfOpenGenericStateApplier = typeof(IApplyEvents<>);
        private static readonly Type typeOfOpenGenericEventSpecificApplier = typeof(IApplyEvent<,>);
        private static readonly Type functionalApplierType = typeof(FunctionalInternalApplier);


        public IManuallyConfigureEventAppliers WithEventApplier<TState, TEvent>(IApplyEvent<TState, TEvent> stateApplier)
            => WithEventApplier(typeof(TState), typeof(TEvent), stateApplier);

        public IManuallyConfigureEventAppliers WithEventApplier(Type stateType, Type eventType, object applier)
            => ManuallyConfigureEventAppliers(stateType, applier,
                typeOfOpenGenericEventSpecificApplier, stateType, eventType);

        public IManuallyConfigureEventAppliers WithEventApplier<TState>(IApplyEvents<TState> stateApplier)
            => WithEventApplier(typeof(TState), stateApplier);

        public IManuallyConfigureEventAppliers WithEventApplier(Type stateType, object applier)
            => ManuallyConfigureEventAppliers(stateType, applier,
                typeOfOpenGenericStateApplier, stateType);

        private IManuallyConfigureEventAppliers ManuallyConfigureEventAppliers(Type stateType,
            object applier,
            Type openApplierType, params Type[] genericTypes)
        {
            //var fromMethod = functionalApplierType.GetMethod("From", BindingFlags.Static | BindingFlags.Public, null, CallingConventions.Any, new[] {openApplierType}, null);
            var fromMethod = functionalApplierType.GetMethods()
                .Where(x => x.Name == "From")
                .SelectMany(x => x.GetParameters().Select(p => new {Method = x, Parameter = p, ParamterType = p.ParameterType}))
                .FirstOrDefault(x=> x.ParamterType.GetGenericTypeDefinition() == openApplierType);

            //TODO: Add check here.

            var result = fromMethod.Method.MakeGenericMethod(genericTypes).Invoke(null, new[] {applier});
            applierCollection.Add(new ApplierRetriever(stateType, result as IApplyEventsInternal));
            return this;
        }

        internal IManuallyConfigureEventAppliers WithImplementationOfSpecificEventApplierOLD(Type stateType, Type eventType, object applier)
        {
            var constructedApplier = typeOfOpenGenericEventSpecificApplier.MakeGenericType(stateType, eventType);

            if(!constructedApplier.IsInstanceOfType(applier)) throw new ArgumentException($"Provided applier with type {applier.GetType().Name} does not implement {constructedApplier.Name}");

            Func<Type,Type,bool> canApplyFunc = (s, e) => s == stateType && (e == eventType || e.IsSubclassOf(eventType));

            var applyMethodInfo = constructedApplier.GetMethod("Apply");

            var objectStateParam = ParameterExpression.Parameter(typeof(object), "state");
            var objectEventParam = ParameterExpression.Parameter(typeof(object), "@event");
            
            var instanceExp = Expression.Constant(applier);
            var applyExp = Expression.Call(instanceExp, applyMethodInfo, Expression.Convert(objectStateParam, stateType), Expression.Convert(objectEventParam, eventType));
            var applyLambdaExp = Expression.Lambda<Func<object, object, object>>(applyExp, objectStateParam, objectEventParam);

            applierCollection.Add(new ApplierRetriever(stateType, new FunctionalInternalApplier(applyLambdaExp.Compile(), canApplyFunc)));

            return this;
        }

        public IConfigureUpconverter AndNoMoreAppliers()
            => baseConfiguration.WithEventApplier(BuildEventApplierFrom(applierCollection.ToArray()));

        private static IApplyEventsToStates BuildEventApplierFrom(ICollection<ApplierRetriever> appliers)
        {
            var applier = new EventApplier();
            applier.SeedWith(appliers);
            return applier;
        }
    }
}