namespace BullOak.Application
{
    using System;
    using System.Reflection;
    using BullOak.Common;
    using BullOak.Messages;
    using Exceptions;
    using MethodBuilderContainer;

    public abstract class Entity : IPersistThroughEvents
    {
        private static readonly ICacheMethods applyInternalMethodCache;

        internal Action<string, IParcelVisionEvent> verboseLoggingFunc;
        internal Action<string, IParcelVisionEvent, Guid> eventRaiseLoggingFunc;

        static Entity()
        {
            applyInternalMethodCache =
                new CachedMethodWithDefaultGenericBuilder(typeof(Entity).GetMethod(nameof(Entity.ApplyInternal),
                    BindingFlags.Instance | BindingFlags.NonPublic));
        }

        internal Entity()
        {
        }

        void IPersistThroughEvents.ReconstituteFrom<TEvent>(TEvent @event)
        {
            if (typeof(TEvent) == @event.GetType())
            {
                ApplyInternal(@event, false);
            }
            else
            {
                applyInternalMethodCache.Invoke(this, @event, false);
            }
        }

        internal abstract void StoreEventInStream(Lazy<IParcelVisionEventEnvelope> lazyEventEnvelope);

        internal abstract void TryStoreSelfInAggregate<TEntityId>(Entity<TEntityId> entity)
            where TEntityId : IId, IEquatable<TEntityId>;

        protected abstract void ApplyEvent<TEvent>(TEvent @event)
            where TEvent : IParcelVisionEvent;

        internal virtual void ApplyInternal<TEvent>(TEvent @event, bool addToStream)
            where TEvent : IParcelVisionEvent
        {
            var thisAsPublisher = this as IPublish<TEvent>;

            if (thisAsPublisher == null)
            {
                throw new EventNotSupportedException(GetType(), GetType(), @event.GetType());
            }

            if(addToStream)
            {
                var lazyEventEnvelope = GetEnvelopeFor(@event);

                eventRaiseLoggingFunc?.Invoke($"Raising {{@event}} {@event.GetType().Name} with {{CorrelationId}}", @event, @event.CorrelationId);

                StoreEventInStream(lazyEventEnvelope);
            }

            verboseLoggingFunc?.Invoke($"Applying {{@event}} {@event.GetType().Name}", @event);
            thisAsPublisher.Apply(@event);
        }

        internal abstract Lazy<IParcelVisionEventEnvelope> GetEnvelopeFor<TEvent>(TEvent @event)
            where TEvent : IParcelVisionEvent;

        internal abstract ParcelVisionEventEnvelope<TChildId> CreateParentEnvelope<TChildId, TEvent>(TEvent @event)
            where TEvent : IParcelVisionEvent;
    }


    public abstract class Entity<TId> : Entity, IHaveAnId<TId>
        where TId : IId, IEquatable<TId>
    {
        public TId Id { get; protected set; }

        //Having it static is OK due to the generic nature of this class and the fact that each entity is meant to have 
        private static readonly ICacheMethods applyInternalMethodCache;

        static Entity()
        {
            applyInternalMethodCache =
                new CachedMethodWithDefaultGenericBuilder(typeof(Entity).GetMethod(nameof(Entity.ApplyInternal),
                    BindingFlags.Instance | BindingFlags.NonPublic));
        }

        internal Entity()
        { }

        protected sealed override void ApplyEvent<TEvent>(TEvent @event)
        {
            if (@event.GetType() != typeof(TEvent))
            {
                applyInternalMethodCache.Invoke(this, @event, true);
            }
            else
            {
                ApplyInternal(@event, true);
            }

            TryStoreSelfInAggregate(this);
        }

        internal sealed override ParcelVisionEventEnvelope<TChildId> CreateParentEnvelope<TChildId, TEvent>(
            TEvent @event)
        {
            return new ParcelVisionEventEnvelope<TChildId, TId, TEvent>()
            {
                ParentId = Id,
                EventRaw = @event,
            };
        }
    }
}