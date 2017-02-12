namespace BullOak.Application.Test.Unit.Aggregate
{
    using System;
    using BullOak.Application.Test.Unit.Aggregate.Events;
    using System.Collections.Generic;

    public class EntityTest : ChildEntity<EntityId, AggregateRootTest>,
        IHaveChildEntities<SubChildEntityTest, SubEntityId>,
        IPublish<EntityAdded>,
        IPublish<EntityUpdated> 
    {
        public string Name { get; private set; }

        public Dictionary<SubEntityId, SubChildEntityTest> SubEntities { get; } = new Dictionary<SubEntityId, SubChildEntityTest>();
    
        public EntityTest() 
        {
        }

        public EntityTest(
                EntityId id,           
                string name, 
                Guid correlationId)
        {
            var @event = new EntityAdded(correlationId, id, name);
            ApplyEvent(@event);
        }

        public SubChildEntityTest GetOrAdd(SubEntityId id, Func<SubEntityId, SubChildEntityTest> factory)
        {
            SubChildEntityTest entity;

            if (!SubEntities.TryGetValue(id, out entity))
            {
                entity = factory(id);

                SubEntities.Add(id, entity);
            }

            return entity;
        }
        public void Update(string name, Guid correlationId)
        {
            if (Name == name) return;

            var @event = new EntityUpdated(correlationId, Parent.Id, Id, name);
            ApplyEvent(@event);
        }

        public void AddChild(SubEntityId subEntityId, string name, Guid correlationId)
        {
            //This is the second way of setting entity relationships; namely passing the parent in the ctor.
            // While this is supported, as you can see, it looks strange. It is useful in situation where the
            // reference is actually kept and used later. It is also useful in situations where we do not want
            // the overhead of caching event published, since if we setup entity relationships, they go directly
            // to the aggregate.
            new SubChildEntityTest(this, subEntityId, name, correlationId);
        }

        void IPublish<EntityAdded>.Apply(EntityAdded @event)
        {
            Id = @event.EntityId;
            Name = @event.Name;
        }

        void IPublish<EntityUpdated>.Apply(EntityUpdated @event)
        {
            Name = @event.Name;
        }
    }
}
