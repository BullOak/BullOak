namespace BullOak.Application
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reflection;
    using BullOak.Messages;
    using Exceptions;
    using MethodBuilderContainer;
    using BullOak.Common;
    using BullOak.EventStream;

    public abstract class AggregateRoot<TId> : Entity<TId>, IOwnAggregateEventStream
        where TId : IId, IEquatable<TId>
    {
        private readonly ConcurrentDictionary<Type, object> childContainers = new ConcurrentDictionary<Type, object>();
        private readonly ICacheMethods reconstituteMethodCache;

        public int ConcurrencyId { get; private set; }

        private readonly IList<Lazy<IParcelVisionEventEnvelope>> aggregateEvents = new List<Lazy<IParcelVisionEventEnvelope>>();

        protected AggregateRoot()
        {
            reconstituteMethodCache =
                new CachedMethodWithTypeSelectorBuilder(
                    typeof(AggregateRoot<TId>).GetMethod(nameof(AggregateRoot<TId>.GetOrCreateAndReconstituteEntity),
                        BindingFlags.Instance | BindingFlags.NonPublic),
                    (object[] objs) =>
                    {
                        var asHasIdTypes = objs[0] as IHaveSourceAndParentIdTypes;

                        if (asHasIdTypes != null) return new[] {asHasIdTypes.SourceIdType, asHasIdTypes.ParentIdType};

                        throw new Exception("Should never ever happen");
                    });
        }

        public void ReconstituteAggregate(IEnumerable<IParcelVisionEventEnvelope> eventStream, int concurrencyId)
        {
            foreach (var eventEnvelope in eventStream)
            {
                var envelopeWithSourceId = eventEnvelope as IParcelVisionEventEnvelope<TId>;

                if (eventEnvelope.SourceEntityType == GetType() || envelopeWithSourceId?.SourceId.Equals(Id) == true)
                {
                    ReconstituteFrom(eventEnvelope);

                    var childContainer = new ConcurrentDictionary<TId, IPersistThroughEvents>(1, 1);
                    childContainers[Id.GetType()] = childContainer;
                    childContainer[Id] = this;
                }
                else
                {
                    var asHasIdTypes = eventEnvelope as IHaveSourceAndParentIdTypes;

                    if (asHasIdTypes == null)
                    {
                        //This should not happen since there is only one sealed implementation of the event envelope.
                        throw new Exception($"Event envelope of type {eventEnvelope.GetType().FullName} does not implement {nameof(IHaveSourceAndParentIdTypes)}");
                    }

                    reconstituteMethodCache.Invoke(this, eventEnvelope);
                }
            }

            ConcurrencyId = concurrencyId;
        }

        private void ReconstituteFrom(IParcelVisionEventEnvelope eventEnvelope)
        {
            ((IPersistThroughEvents)this).ReconstituteFrom(eventEnvelope.Event);
        }

        private void GetOrCreateAndReconstituteEntity<TChildId, TParentId>(IParcelVisionEventEnvelope<TChildId, TParentId> eventEnvelope)
            where TParentId: IId, IEquatable<TParentId>
            where TChildId : IId, IEquatable<TChildId>
        {
            IPersistThroughEvents entity;

            var childContainer = GetOrCreateContainer<TChildId>();

            if (!childContainer.TryGetValue(eventEnvelope.SourceId, out entity))
            {
                entity = CreateAndSetupRelationships(eventEnvelope, entity);
                childContainer[eventEnvelope.SourceId] = entity;
            }

            entity.ReconstituteFrom(eventEnvelope.Event);
        }

        private IPersistThroughEvents CreateAndSetupRelationships<TChildId, TParentId>(IParcelVisionEventEnvelope<TChildId, TParentId> eventEnvelope,
                IPersistThroughEvents entity)
            where TParentId : IId, IEquatable<TParentId>
            where TChildId : IId, IEquatable<TChildId>
        {
            var parent = GetParentOrThrow(eventEnvelope);

            var newChildEntity = (IPersistThroughEvents)Activator.CreateInstance(eventEnvelope.SourceEntityType);
            var parentAsEntity = parent as Entity<TParentId>;
            
            var childAsHasParent = newChildEntity as IHaveAParent;
            if (childAsHasParent != null) SetParentInChild(parentAsEntity, childAsHasParent);

            entity = newChildEntity;
            return entity;
        }

        private IPersistThroughEvents GetParentOrThrow<TChildId, TParentId>(IParcelVisionEventEnvelope<TChildId, TParentId> eventEnvelope)
            where TParentId : IId 
            where TChildId : IId 
        {
            IPersistThroughEvents parent;
            var parentContainer = GetOrCreateContainer<TParentId>();

            //TODO (Savvas) replace with propper exception
            if (!parentContainer.TryGetValue(eventEnvelope.ParentId, out parent))
                throw new Exception("Parent should exist already");
            return parent;
        }

        private void SetParentInChild<TParent, TChild>(TParent parent, TChild child)
            where TChild: IHaveAParent
            where TParent: Entity
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (child == null) throw new ArgumentNullException(nameof(child));

            child.SetParent(parent);
        }

        /// <summary>
        /// This method attempts a retrieval on an entity that is contained by the aggregate (and not just direct childs of the aggregate root)
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TEntityId"></typeparam>
        /// <param name="id"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected bool TryGetEntity<TEntity, TEntityId>(TEntityId id, out TEntity entity)
            where TEntityId: IId, IEquatable<TEntityId>
            where TEntity: Entity<TEntityId>
        {
            var childContainer = childContainers[id.GetType()] as IDictionary<TEntityId, IPersistThroughEvents>;

            IPersistThroughEvents persistThroughEvents = null;

            childContainer?.TryGetValue(id, out persistThroughEvents);

            entity = persistThroughEvents as TEntity;

            return entity != null;
        }

        void IOwnAggregateEventStream.ClearUncommitedEvents() => aggregateEvents.Clear();
        IParcelVisionEventEnvelope[] IOwnAggregateEventStream.GetUncommitedEventsForAggregate()
            => aggregateEvents.Select(x => x.Value).ToArray();

        internal sealed override void StoreEventInStream(Lazy<IParcelVisionEventEnvelope> lazyEventEnvelope) => aggregateEvents.Add(lazyEventEnvelope);
        internal sealed override void TryStoreSelfInAggregate<TEntityId>(Entity<TEntityId> entity)
        {
            var childContainer = GetOrCreateContainer<TEntityId>();

            var existing = childContainer.GetOrAdd(entity.Id, entity);

            if (existing != entity) throw new EntityExistsException(entity.Id.ToString(), entity.GetType(), Id);
        }

        internal sealed override Lazy<IParcelVisionEventEnvelope> GetEnvelopeFor<TEvent>(TEvent @event)
        {
            return new Lazy<IParcelVisionEventEnvelope>(() => new ParcelVisionEventEnvelope<TId, TId, TEvent>()
            {
                EventRaw = @event,
                ParentId = Id,
                SourceId = Id,
                SourceEntityType = GetType()
            });
        }

        private ConcurrentDictionary<TParentId, IPersistThroughEvents> GetOrCreateContainer<TParentId>() => 
            (ConcurrentDictionary<TParentId, IPersistThroughEvents>) childContainers.GetOrAdd(typeof(TParentId),
                t => new ConcurrentDictionary<TParentId, IPersistThroughEvents>());
    }
}