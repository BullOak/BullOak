﻿namespace BullOak.Repositories.Session
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BullOak.Repositories.Appliers;
    using BullOak.Repositories.EventPublisher;
    using BullOak.Repositories.Middleware;
    using BullOak.Repositories.StateEmit;

    public abstract class BaseRepoSession<TState> : IManageSessionOf<TState>
    {
        private readonly IDisposable disposableHandle;

        protected readonly IHoldAllConfiguration configuration;
        protected ICollection<ItemWithType> NewEventsCollection { get; private set; }

        private TState currentState;
        public TState GetCurrentState() => currentState;

        protected readonly IValidateState<TState> stateValidator = new AlwaysPassValidator<TState>();
        public IValidateState<TState> StateValidator => stateValidator;

        protected readonly IPublishEvents eventPublisher;

        private static readonly Type stateType = typeof(TState);
        protected readonly IApplyEventsToStates EventApplier;

        public bool IsNewState { get; private set; }

        protected BaseRepoSession(IHoldAllConfiguration configuration, IDisposable disposableHandle)
            : this(new AlwaysPassValidator<TState>(), configuration, disposableHandle)
        { }

        protected BaseRepoSession(IValidateState<TState> validator,
            IHoldAllConfiguration configuration,
            IDisposable disposableHandle)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            this.disposableHandle = disposableHandle;

            NewEventsCollection = configuration.CollectionTypeSelector(stateType)();

            currentState = default(TState);

            this.eventPublisher = configuration.EventPublisher;

            EventApplier = this.configuration.EventApplier;
            stateValidator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public void AddEvents(IEnumerable<object> events)
        {
            if (events == null) throw new ArgumentNullException(nameof(events));

            foreach (var @event in events.Select(x=> new ItemWithType(x)))
                NewEventsCollection.Add(@event);
            
            currentState =
                (TState) EventApplier.Apply(stateType, currentState, events.Select(x => new ItemWithType(x)));
        }

        public void AddEvents(object[] events)
        {
            if (events == null) throw new ArgumentNullException(nameof(events));

            for (int i = 0; i < events.Length; i++)
                NewEventsCollection.Add(new ItemWithType(events[i]));

            currentState =
                (TState) EventApplier.Apply(stateType, currentState, events.Select(x => new ItemWithType(x)));
        }

        public void AddEvent(object @event)
        {
            if (@event is ICollection eventCollection)
            {
                object[] eventArray = new object[eventCollection.Count];
                eventCollection.CopyTo(eventArray, 0);
                AddEvents(eventArray);
            }
            else if (@event is IEnumerable<object> events)
                AddEvents(events);
            else
                AddEventInternal(new ItemWithType(@event));
        }

        public void AddEvent<TEvent>(Action<TEvent> initializeEventAction)
        {
            var @event = (TEvent)configuration.StateFactory.GetState(typeof(TEvent));

            var switchable = @event as ICanSwitchBackAndToReadOnly;

            if (switchable != null) switchable.CanEdit = true;
            initializeEventAction(@event);
            if (switchable != null) switchable.CanEdit = false;
            AddEventInternal(new ItemWithType(@event, typeof(TEvent)));
        }

        private void AddEventInternal(ItemWithType @event)
        {
            NewEventsCollection.Add(@event);
            currentState = (TState) EventApplier.ApplyEvent(stateType, currentState, @event);
        }

        protected void Initialize(TState storedState, bool isNew)
        {
            currentState = storedState;
            IsNewState = isNew;
        }

        public async Task<int> SaveChanges(DeliveryTargetGuarntee targetGuarantee =
                DeliveryTargetGuarntee.AtLeastOnce,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            ValidateStateOrThrow(stateValidator, currentState);

            var newEvents = NewEventsCollection.ToArray();
            var sendEventsBeforeSaving = targetGuarantee == DeliveryTargetGuarntee.AtLeastOnce;

            if (sendEventsBeforeSaving) await PublishEvents(configuration, newEvents, cancellationToken);

            bool hasInterceptors = configuration.HasInterceptors;

            for(int i = 0 ;i<newEvents.Length;i++)
            {
                TryCallInterceptor(hasInterceptors, configuration.Interceptors, newEvents[i],
                    (interc, e, et, s, st) => interc.BeforeSave(e, et, s, st));
            }

            var retVal = await SaveChanges(newEvents, currentState, cancellationToken);
            NewEventsCollection.Clear();

            for (int i = 0; i < newEvents.Length; i++)
            {
                TryCallInterceptor(hasInterceptors, configuration.Interceptors, newEvents[i],
                    (interc, e, et, s, st) => interc.AfterSave(e, et, s, st));
            }


            if (!sendEventsBeforeSaving) await PublishEvents(configuration, newEvents, cancellationToken);

            return retVal;
        }

        private static void ValidateStateOrThrow(IValidateState<TState> validator, TState state)
        {
            var valResults = validator.Validate(state);

            if (!valResults.IsSuccess)
            {
                var errors = valResults.ValidationErrors.ToArray();

                throw new AggregateException(errors.Select(e => e.GetAsException()));
            }
        }

        protected abstract Task<int> SaveChanges(ItemWithType[] newEvents,
            TState currentState,
            CancellationToken? cancellationToken);

        private async Task PublishEvents(IHoldAllConfiguration configuration, ItemWithType[] events, CancellationToken cancellationToken = default(CancellationToken))
        {
            bool hasInterceptors = configuration.HasInterceptors;
            ItemWithType @event;
            for (var i = 0; i < events.Length; i++)
            {
                @event = events[i];

                TryCallInterceptor(hasInterceptors, configuration.Interceptors, @event,
                    (interc, e, et, s, st) => interc.BeforePublish(e, et, s, st));

                //We await each one to guarantee ordering of publishing, even though it would have been more performant
                // to publish and await all of them with a WhenAll
                await eventPublisher.Publish(events[i], cancellationToken);

                TryCallInterceptor(hasInterceptors, configuration.Interceptors, @event,
                    (interc, e, et, s, st) => interc.AfterPublish(e, et, s, st));
            }
        }

        private void TryCallInterceptor(bool hasInterceptors, IInterceptEvents[] interceptors,
            ItemWithType @event, Action<IInterceptEvents, object, Type, object, Type> interceptorMethod)
        {
            if (hasInterceptors)
            {
                var interceptorCount = interceptors?.Length ?? 0;
                IInterceptEvents interceptor;
                for (int j = 0; j < interceptorCount; j++)
                {
                    interceptor = interceptors[j];
                    interceptorMethod(interceptor, @event.instance, @event.type, currentState, stateType);
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if(disposing) disposableHandle?.Dispose();
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
