namespace BullOak.Application.Test.Unit.Aggregate
{
    using System;
    using BullOak.Application.Test.Unit.Aggregate.Events;
    using Application;
    using System.Collections.Generic;
    using BullOak.Application.Exceptions;
    using BullOak.Common;
    using BullOak.Messages;

    public class AggregateRootTest : AggregateRoot<AggregateRootTestId>,
        IHaveChildEntities<EntityTest, EntityId>,
        IPublish<AggregateCreated>,
        IPublish<AggregateUpdated>         
    {
        public string Name { get; private set; }
        public Dictionary<EntityId, EntityTest> Entities { get; set; }

        public AggregateRootTest()
        {
            Entities = new Dictionary<EntityId, EntityTest>();
        }

        public AggregateRootTest(AggregateRootTestId id, string name, Guid correlationId)
            : this()
        {
            var @event = new AggregateCreated(correlationId, id, name);
            ApplyEvent(@event);
        }

        #region Action methods
        public void StoreEvent(IParcelVisionEvent @event)
        {
            if (@event is AggregateUpdated)
            {
                ApplyEvent(@event);
            }
            else
            {
                throw new Exception($"Sorry only {nameof(AggregateUpdated)} events are currently supported");
            }
        }

        public void Update(string name, Guid correlationId)
        {
            var @event = new AggregateUpdated(correlationId, Id, name);
            ApplyEvent(@event);
        }

        public void AddChild(EntityId entityId, string name, Guid correlationId)
        {
            var entity = new EntityTest(entityId, name, correlationId);
            entity.SetParent(this);
        }

        public void UpdateChild(EntityId entityId, string name, Guid correlationId)
        {
            //NOTE 1: Logic to manipulate child entities ALWAYS goes through the aggregate root
            //NOTE 2: This uses a custom array to retrieve entities. This is optional and is populated
            // through the use of IHaveChildEntities, and is possible on all entities. Aggregate roots
            // specifically have an additional option: using the TryGetEntity method which can retrieve
            // any entity inside the aggregate and not just direct childs of the aggregate.
            var entity = Entities[entityId];

            entity.Update(name, correlationId);
        }

        public void AddSubChild(EntityId parentId, SubEntityId subEntityId, string name, Guid correlationId)
        {
            EntityTest parent;
            if (TryGetEntity(parentId, out parent))
            {
                parent.AddChild(subEntityId, name, correlationId);
            }
            else
            {
                throw new EntityNotFoundException(parentId.ToString(), typeof(EntityTest));
            }
        }

        public void UpdateSubChild(SubEntityId childId, string name, Guid correlationId)
        {
            // logic to manipulate child entities ALWAYS goes through the aggregate root
            var entity = GetAggregateEntity<SubEntityId, SubChildEntityTest>(childId); 

            entity.Update(name, correlationId);
        }

        public TEntity GetAggregateEntity<TEntityId, TEntity>(TEntityId entityId)
            where TEntity: Entity<TEntityId>
            where TEntityId: struct, IId, IEquatable<TEntityId>
        {
            TEntity entity;

            //This is the method that exists in all aggregate roots which allows access to any and all
            // entities that that aggregate contains.
            TryGetEntity(entityId, out entity);

            return entity;
        }
        #endregion

        void IPublish<AggregateCreated>.Apply(AggregateCreated @event)
        {
            Id = @event.AggregateSutId;
            Name = @event.Name;
        }

        void IPublish<AggregateUpdated>.Apply(AggregateUpdated @event)
        {
            Name = @event.Name;
        }

        EntityTest IHaveChildEntities<EntityTest, EntityId>.GetOrAdd(EntityId id, Func<EntityId, EntityTest> factory)
        {
            EntityTest entity;

            if (!Entities.TryGetValue(id, out entity))
            {
                entity = factory(id);

                Entities.Add(id, entity);
            }

            return entity;
        }
    }
} 
