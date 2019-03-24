namespace BullOak.Repositories.Test.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BullOak.Repositories.Appliers;
    using BullOak.Repositories.EventPublisher;
    using BullOak.Repositories.InMemory;
    using BullOak.Repositories.Middleware;
    using BullOak.Repositories.Rehydration;
    using BullOak.Repositories.StateEmit;
    using BullOak.Repositories.Upconverting;
    using FakeItEasy;

    public class ConfigurationStub<TState> : IHoldAllConfiguration
    {
        private static readonly Type typeOfGenericList = typeof(List<>);

        private readonly List<Type> typesForWhichEventCollectionHasBeenAskedFor;
        private readonly List<object> eventsThatHaveBeenPublished;
        private readonly List<Type> typesThatHaveAskedForSafetyType;
        private readonly Fake<IApplyEventsToStates> mockEventApplier;
        private readonly Fake<ICreateStateInstances> mockStateFactory;

        public List<Type> TypesForWhichEventCollectionHasBeenAskedFor => typesForWhichEventCollectionHasBeenAskedFor;
        public List<object> EventsThatHaveBeenPublished => eventsThatHaveBeenPublished;
        public List<Type> TypesThatHaveAskedForSafetyType => typesThatHaveAskedForSafetyType;
        public Fake<IApplyEventsToStates> MockEventApplier => mockEventApplier;
        public Fake<ICreateStateInstances> MockStateFactory => mockStateFactory;

        public Func<Type, Func<ICollection<ItemWithType>>> CollectionTypeSelector { get; private set; }
        public IPublishEvents EventPublisher { get; private set; }
        private IApplyEventsToStates eventApplier = null;

        public IApplyEventsToStates EventApplier
        {
            get => eventApplier ?? MockEventApplier.FakedObject;
            set => eventApplier = value;
        }

        public Func<Type, bool> ThreadSafetySelector { get; private set; }
        private ICreateStateInstances stateFactory = null;
        public ICreateStateInstances StateFactory => stateFactory ?? MockStateFactory.FakedObject;
        public IUpconvertStoredItems EventUpconverter { get; private set; }

        private List<IInterceptEvents> InterceptorList = new List<IInterceptEvents>();
        public bool HasInterceptors => Interceptors.Length > 0;
        public IInterceptEvents[] Interceptors => InterceptorList.ToArray();

        public IRehydrateState StateRehydrator { get; private set; }

        public ConfigurationStub()
        {
            mockStateFactory = new Fake<ICreateStateInstances>();
            mockEventApplier = new Fake<IApplyEventsToStates>();
            EventUpconverter = new NullUpconverter();

            typesForWhichEventCollectionHasBeenAskedFor = new List<Type>();
            eventsThatHaveBeenPublished = new List<object>();
            typesThatHaveAskedForSafetyType = new List<Type>();

            StateRehydrator = new Rehydrator(this);
        }

        public ConfigurationStub<TState> WithDefaultSetup()
        {
            EventPublisher = new MySyncEventPublisher(e =>
            {
                eventsThatHaveBeenPublished.Add(e.instance);
            });
            WithCollectionType<List<object>>();
            WithThreadSafety(false);
            WithStateFactory(Activator.CreateInstance);
            WithJustReturnEventApplier();

            return this;
        }

        public ConfigurationStub<TState> WithEventApplier(IApplyEventsToStates applier)
        {
            this.EventApplier = applier;
            return this;
        }

        private void WithJustReturnEventApplier()
        {
            mockEventApplier.CallsTo(a => a.ApplyEvent(typeof(object), new object(), new ItemWithType()))
                .WithAnyArguments()
                .ReturnsLazily(c => c.Arguments.ToArray()[1]);
            mockEventApplier.CallsTo(a => a.Apply(typeof(object), new object(), new ItemWithType[0]))
                .WithAnyArguments()
                .ReturnsLazily(c => c.Arguments.ToArray()[1]);
            mockEventApplier.CallsTo(a => a.Apply(typeof(object), new object(), new List<ItemWithType>()))
                .WithAnyArguments()
                .ReturnsLazily(c => c.Arguments.ToArray()[1]);
        }

        public ConfigurationStub<TState> WithCollectionType<TCollection>()
        {
            CollectionTypeSelector = t =>
            {
                typesForWhichEventCollectionHasBeenAskedFor.Add(t);
                return () => new List<ItemWithType>();
            };
            return this;
        }

        public ConfigurationStub<TState> WithInterceptor(IInterceptEvents interceptor)
        {
            InterceptorList.Add(interceptor);
            return this;
        }

        public ConfigurationStub<TState> WithThreadSafety(bool threadSafety)
        {
            ThreadSafetySelector = t =>
            {
                TypesThatHaveAskedForSafetyType.Add(t);
                return threadSafety;
            };
            return this;
        }

        public ConfigurationStub<TState> WithStateFactory(Func<Type, object> factory)
        {
            MockStateFactory.CallsTo(i => i.GetState(null))
                .WithAnyArguments()
                .ReturnsLazily(t => factory((Type) t.Arguments[0]));

            return this;
        }

        public ConfigurationStub<TState> WithDefaultStateFactory()
        {
            this.stateFactory = new EmittedTypeFactory();
            return this;
        }

        internal InMemoryEventSourcedRepository<TStateId, TState> GetNewSUT<TStateId>()
            => new InMemoryEventSourcedRepository<TStateId, TState>(this);

        public ConfigurationStub<TState> WithEventPublisher(IPublishEvents eventPublisher)
        {
            this.EventPublisher = eventPublisher;
            return this;
        }
    }
}