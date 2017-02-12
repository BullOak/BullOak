namespace BullOak.Application.Test.Unit.Aggregate
{
    using System;
    using BullOak.Application.Test.Unit.Aggregate.Events;

    public interface ISubEntitySut
    {
        AggregateRootTestId Id { get; }

        string Name { get; }
    } 

    public class SubChildEntityTest : ChildEntity<SubEntityId, EntityTest>, 
        IPublish<SubEntityCreated>,
        IPublish<SubEntityUpdated>
    { 
        public string Name { get; private set; }

        public SubChildEntityTest()
        { }

        public SubChildEntityTest(EntityTest parent,
                SubEntityId id,
                string name,
                Guid correlationId) 
            :base(parent)
        {
            var @event = new SubEntityCreated(correlationId, id, name);

            ApplyEvent(@event);
        }

        public void Update(string name, Guid correlationId)
        {
            if (Name == name) return;

            var @event = new SubEntityUpdated(correlationId, Id, name);
            ApplyEvent(@event);
        }

        void IPublish<SubEntityCreated>.Apply(SubEntityCreated @event)
        {
            Id = @event.SubEntityId;
            Name = @event.Name;
        }

        void IPublish<SubEntityUpdated>.Apply(SubEntityUpdated @event)
        {
            Name = @event.Name;
        }
    }
}
