namespace BullOak.Application.Test.Unit
{
    using System;
    using System.Collections.Generic;
    using BullOak.Application.Test.Unit.Aggregate;
    using BullOak.Messages;
    using System.Linq;
    using System.Threading.Tasks;
    using BullOak.Application.Test.Unit.Aggregate.Events;
    using FluentAssertions;
    using Xunit;
    using BullOak.Application.Exceptions;
    using BullOak.EventStream;
    using BullOak.Infrastructure.TestHelpers.Application.Stubs;

    public class AggregateSutTests
    {
        private class Arrangements
        {
            public Guid CorrelationId { get; } = Guid.NewGuid();

            public EntityId ChildEntityA { get; } = (EntityId)Guid.NewGuid();

            public EntityId ChildEntityB { get; } = (EntityId)Guid.NewGuid();

            public SubEntityId ChildSubEntityAA { get; } = (SubEntityId)Guid.NewGuid();

            public SubEntityId ChildSubEntityAB { get; } = (SubEntityId)Guid.NewGuid();

            public AggregateRootTestId AggregateSutId { get; } = (AggregateRootTestId)Guid.NewGuid();

            public AggregateRootTestRepo Repository { get; set; }

            public InMemoryEventStore EventStore { get; set; }

            public List<IParcelVisionEventEnvelope> Events => EventStore[AggregateSutId.ToString()];

            public void AddCreationEvent()
            {
                Events.Add(new ParcelVisionEventEnvelope<AggregateRootTestId, AggregateRootTestId, AggregateCreated>
                {
                    SourceId = AggregateSutId,
                    ParentId = AggregateSutId,
                    SourceEntityType = typeof(AggregateRootTest),
                    EventRaw = new AggregateCreated(CorrelationId, AggregateSutId, "ABC123")
                });
            }

            public void AddUpdatedEvent()
            {
                Events.Add(new ParcelVisionEventEnvelope<AggregateRootTestId, AggregateRootTestId, AggregateUpdated>
                {
                    SourceId = AggregateSutId,
                    ParentId = AggregateSutId,
                    SourceEntityType = typeof(AggregateRootTest),
                    EventRaw = new AggregateUpdated(CorrelationId, AggregateSutId, "abc777")
                });
            }

            public void AddDeletedEvent()
            {
                Events.Add(new ParcelVisionEventEnvelope<AggregateRootTestId, AggregateRootTestId, AggregateDeleted>
                {
                    SourceId = AggregateSutId,
                    ParentId = AggregateSutId,
                    SourceEntityType = typeof(AggregateRootTest),
                    EventRaw = new AggregateDeleted(CorrelationId, AggregateSutId)
                });
            }

            public void AddChildEvent(EntityId? entityId = null, string name = null)
            {
                Events.Add(new ParcelVisionEventEnvelope<EntityId, AggregateRootTestId, EntityAdded>
                {
                    SourceId = entityId ?? ChildEntityA,
                    ParentId = AggregateSutId,
                    SourceEntityType = typeof(EntityTest),
                    EventRaw = new EntityAdded(CorrelationId, entityId ?? ChildEntityA, name ?? "My name is Bond")
                });
            }

            public void AddChild2Event()
            {
                Events.Add(new ParcelVisionEventEnvelope<EntityId, AggregateRootTestId, EntityAdded>
                {
                    SourceId = ChildEntityB,
                    ParentId = AggregateSutId,
                    SourceEntityType = typeof(EntityTest),
                    EventRaw = new EntityAdded(CorrelationId, ChildEntityB, "Dr Doolittle")
                });
            }

            public void AddSubChildCreatedEvent(SubEntityId? subChildId = null, EntityId? parentId = null, string name = "My name is Q")
            {
                Events.Add(new ParcelVisionEventEnvelope<SubEntityId, EntityId, SubEntityCreated>
                {
                    SourceId = subChildId ?? ChildSubEntityAA,
                    ParentId = parentId ?? ChildEntityA,
                    SourceEntityType = typeof(SubChildEntityTest),
                    EventRaw = new SubEntityCreated(CorrelationId, subChildId ?? ChildSubEntityAA, name ?? "My name is Q")
                });
            }

            public void UpdateChildEvent()
            {
                Events.Add(new ParcelVisionEventEnvelope<EntityId, AggregateRootTestId, EntityUpdated>
                {
                    SourceId = ChildEntityA,
                    ParentId = AggregateSutId,
                    SourceEntityType = typeof(EntityTest),
                    EventRaw = new EntityUpdated(CorrelationId, AggregateSutId, ChildEntityA, "James Bond") 
                });
            }

            public void UpdateChildSubEvent()
            {
                Events.Add(new ParcelVisionEventEnvelope<SubEntityId, EntityId, SubEntityUpdated>
                {
                    SourceId = ChildSubEntityAA,
                    ParentId = ChildEntityA,
                    SourceEntityType = typeof(SubChildEntityTest),
                    EventRaw = new SubEntityUpdated(CorrelationId, ChildSubEntityAA, "Jaws")
                });
            }

            public void RemoveChildEvent()
            {
                Events.Add(new ParcelVisionEventEnvelope<EntityId, AggregateRootTestId, EntityRemoved>
                {
                    SourceId = ChildEntityA,
                    ParentId = AggregateSutId,
                    SourceEntityType = typeof(EntityTest),
                    EventRaw = new EntityRemoved(CorrelationId, AggregateSutId, ChildEntityA) 
                });

            }

            //public void RemoveChildSubEvent()
            //{
            //    Events.Add(new ParcelVisionEventEnvelope<SubEntityId, EntityId>
            //    {
            //        SourceId = ChildSubEntityAA,
            //        ParentId = ChildEntityA,
            //        SourceEntityType = typeof(SubChildEntityTest),
            //        Event = new SubEntityRemoved(CorrelationId, AggregateSutId, ChildSubEntityAA)
            //    });

            //}

            public void AddNotImplementedAggregateEvent()
            {
                Events.Add(new ParcelVisionEventEnvelope<AggregateRootTestId, AggregateRootTestId, NotImplementedAggregateEvent>
                {
                    SourceId = AggregateSutId,
                    ParentId = AggregateSutId,
                    SourceEntityType = typeof(AggregateRootTest),
                    EventRaw = new NotImplementedAggregateEvent(CorrelationId, AggregateSutId)
                });
            }

            public void AddNotImplementedEntityEvent()
            {
                Events.Add(new ParcelVisionEventEnvelope<EntityId, AggregateRootTestId, NotImplementedEntityEvent>
                {
                    SourceId = ChildEntityA,
                    ParentId = AggregateSutId,
                    SourceEntityType = typeof(EntityTest),
                    EventRaw = new NotImplementedEntityEvent(CorrelationId, ChildEntityA, AggregateSutId)
                });
            }

            public void AddAggregateCreatedEvent(AggregateRootTestId id, string aggregateName)
            {
                Events.Add(new ParcelVisionEventEnvelope<AggregateRootTestId, AggregateRootTestId, AggregateCreated>
                {
                    EventRaw = new AggregateCreated(Guid.NewGuid(), id, aggregateName),
                    ParentId = id,
                    SourceId = id,
                    SourceEntityType = typeof(AggregateRootTest)
                });
            }
        }

        private Arrangements GetArrangements()
        {
            var arrangements = new Arrangements()
            {
                EventStore = new InMemoryEventStore(),
            };

            arrangements.Repository = new AggregateRootTestRepo(arrangements.EventStore);

            return arrangements;
        }
       
        [Fact]
        public async Task AggregateSut_Create_ExpectCreatedEvent()
        {
            // Arrange
            var arrangements = GetArrangements();

            // Act
            var sut = new AggregateRootTest(arrangements.AggregateSutId, "Aggregate Root", arrangements.CorrelationId);
            await arrangements.Repository.Save(sut);

            // Assert
            arrangements.Events.Count.Should().Be(1);
            var envelope = arrangements.Events.Single(x=> x.SourceEntityType == typeof(AggregateRootTest));
            var @event = envelope.Event as AggregateCreated;            
            @event.AggregateSutId.Should().Be(arrangements.AggregateSutId);
            @event.Name.Should().Be("Aggregate Root");
            @event.CorrelationId.Should().Be(arrangements.CorrelationId);
        }

        [Fact]
        public async Task AggregateSut_Create_ExpectEnvelopeWithSourceAndParentIdSetToAggregateId()
        {
            // Arrange
            var arrangements = GetArrangements();

            // Act
            var aggregate = new AggregateRootTest(arrangements.AggregateSutId, "Aggregate Root", arrangements.CorrelationId);
            await arrangements.Repository.Save(aggregate);

            var envelope = arrangements.Events.Single();
            envelope.Event.Should().BeOfType<AggregateCreated>();
            envelope.Should().BeOfType<ParcelVisionEventEnvelope<AggregateRootTestId, AggregateRootTestId, AggregateCreated>>();
            envelope.As<ParcelVisionEventEnvelope<AggregateRootTestId, AggregateRootTestId, AggregateCreated>>().SourceId.Should().Be(arrangements.AggregateSutId);
            envelope.As<ParcelVisionEventEnvelope<AggregateRootTestId, AggregateRootTestId, AggregateCreated>>().ParentId.Should().Be(arrangements.AggregateSutId);
            envelope.As<ParcelVisionEventEnvelope<AggregateRootTestId, AggregateRootTestId, AggregateCreated>>().SourceEntityType.Should().Be(typeof(AggregateRootTest));
        }

        [Fact]
        public async Task AggregateSut_ReconstituteWithAggregateCreationEvent_ShouldReturnAggregateWithNameAndId()
        {
            // Arrange
            var arrangements = GetArrangements();
            string aggregateName = "AggregateName";
            arrangements.AddAggregateCreatedEvent(arrangements.AggregateSutId, aggregateName);

            // Act
            var aggregate = await arrangements.Repository.Load(arrangements.AggregateSutId);

            // Assert
            aggregate.Name.Should().Be(aggregateName);
            aggregate.Id.Should().Be(arrangements.AggregateSutId);
        }

        [Fact]
        public async Task AggregateSut_ReconstituteWithAggregateCreationEvent_ShouldHaveConcurrencyIdEquealToOne()
        {
            // Arrange
            var arrangements = GetArrangements();
            string aggregateName = "AggregateName";
            arrangements.AddAggregateCreatedEvent(arrangements.AggregateSutId, aggregateName);

            // Act
            var aggregate = await arrangements.Repository.Load(arrangements.AggregateSutId);

            // Assert
            aggregate.ConcurrencyId.Should().Be(1);
        }

        // NOTE: Added due to a subtle bug. Please keep.
        [Fact]
        public async Task AggregateSut_ReconstituteAggregate_ShouldNotAddLoadedEventsToEventStream()
        {
            // Arrange
            var arrangements = GetArrangements();
            string aggregateName = "AggregateName";
            arrangements.AddAggregateCreatedEvent(arrangements.AggregateSutId, aggregateName);
            var originalEventcount = arrangements.Events.Count;

            // Act
            var aggregate = await arrangements.Repository.Load(arrangements.AggregateSutId);
            await arrangements.Repository.Save(aggregate);

            // Assert
            arrangements.Events.Count.Should().Be(originalEventcount);
        }

        [Fact]
        public async Task AggregateSut_WhenSavingEventWithReferenceOfABaseType_EventEnvelopeHasCorrectEventType()
        {
            // Arrange
            var arrangements = GetArrangements();
            arrangements.AddCreationEvent();
            var sut = await arrangements.Repository.Load(arrangements.AggregateSutId, true);

            // Act
            sut.StoreEvent(new AggregateUpdated(Guid.NewGuid(), arrangements.AggregateSutId, "New name"));
            await arrangements.Repository.Save(sut);

            // Assert
            arrangements.Events.Count.Should().Be(2);
            arrangements.Events[1].Should()
                .BeOfType<ParcelVisionEventEnvelope<AggregateRootTestId, AggregateRootTestId, AggregateUpdated>>();
            arrangements.Events[1]
                .As<ParcelVisionEventEnvelope<AggregateRootTestId, AggregateRootTestId, AggregateUpdated>>()
                .Event.Should().BeOfType<AggregateUpdated>();
        }

        [Fact]
        public async Task AggregateSut_AfterSavingEventWithReferenceOfABaseType_AggregateCanBeReconstitutedCorrectly()
        {
            // Arrange
            var newName = $"NewName{Guid.NewGuid()}";
            var arrangements = GetArrangements();
            arrangements.AddCreationEvent();
            var sut = await arrangements.Repository.Load(arrangements.AggregateSutId, true);
            sut.StoreEvent(new AggregateUpdated(Guid.NewGuid(), arrangements.AggregateSutId, newName));
            await arrangements.Repository.Save(sut);

            // Act
            sut = await arrangements.Repository.Load(arrangements.AggregateSutId, true);

            // Assert
            sut.Name.Should().Be(newName);
        }

        [Fact]
        public async Task AggregateSut_UpdateExisting_ExpectUpdatedAggregate()
        {
            // Arrange
            var arrangements = GetArrangements();
            arrangements.AddCreationEvent();
            var sut = await arrangements.Repository.Load(arrangements.AggregateSutId, true);

            // Act
            sut.Update("XYZ987", arrangements.CorrelationId);

            // Assert
            sut.Name.Should().Be("XYZ987");
        }

        [Fact]
        public async Task AggregateSut_AddChildEntity_ExpectChildCreationEvent()
        {
            // Arrange
            var arrangements = GetArrangements();
            arrangements.AddCreationEvent();
            var sut = await arrangements.Repository.Load(arrangements.AggregateSutId, true);
            string childEntityName = "ChildEntityName";


            // Act
            sut.AddChild(arrangements.ChildEntityA, childEntityName, arrangements.CorrelationId);
            await arrangements.Repository.Save(sut);

            // Assert
            var @event = arrangements.Events.SingleOrDefault(x => x.Event is EntityAdded)?.Event as EntityAdded;
            @event.Should().NotBeNull();
            @event.EntityId.Should().Be(arrangements.ChildEntityA);
            @event.Name.Should().Be(childEntityName);
            @event.CorrelationId.Should().Be(arrangements.CorrelationId);
        }

        [Fact]
        public async Task AggregateSut_AddChildEntity_ExpectChildToBeAddedToAggregateEntities()
        {
            // Arrange
            var arrangements = GetArrangements();
            arrangements.AddCreationEvent();
            var aggregateRoot = await arrangements.Repository.Load(arrangements.AggregateSutId, true);
            string childEntityName = "ChildEntityName";


            // Act
            aggregateRoot.AddChild(arrangements.ChildEntityA, childEntityName, arrangements.CorrelationId);
            await arrangements.Repository.Save(aggregateRoot);
            var child = aggregateRoot.GetAggregateEntity<EntityId, EntityTest>(arrangements.ChildEntityA);

            // Assert
            child.Should().NotBeNull();
            child.Id.Should().Be(arrangements.ChildEntityA);
        }

        [Fact]
        public async Task AggregateSut_AddChildEntity_ExpectChildToHaveParentSet()
        {
            // Arrange
            var arrangements = GetArrangements();
            arrangements.AddCreationEvent();
            var aggregateRoot = await arrangements.Repository.Load(arrangements.AggregateSutId, true);
            string childEntityName = "ChildEntityName";


            // Act
            aggregateRoot.AddChild(arrangements.ChildEntityA, childEntityName, arrangements.CorrelationId);
            await arrangements.Repository.Save(aggregateRoot);
            var child = aggregateRoot.GetAggregateEntity<EntityId, EntityTest>(arrangements.ChildEntityA);

            // Assert
            child.Parent.Should().NotBeNull();
            child.Parent.Id.Should().Be(arrangements.AggregateSutId);
        }

        [Fact]
        public async Task AggregateSut_AddChildEntityWhenImplementingIHaveChildEntities_ExpectAddChildEntityCalled()
        {
            // Arrange
            var arrangements = GetArrangements();
            arrangements.AddCreationEvent();
            var sut = await arrangements.Repository.Load(arrangements.AggregateSutId, true);
            string childEntityName = "ChildEntityName";


            // Act
            sut.AddChild(arrangements.ChildEntityA, childEntityName, arrangements.CorrelationId);

            // Assert
            sut.Entities.Count.Should().Be(1);
            sut.Entities[arrangements.ChildEntityA].Name.Should().Be(childEntityName);
            sut.Entities[arrangements.ChildEntityA].Id.Should().Be(arrangements.ChildEntityA);
        }

        [Fact]
        public async Task AggregateSut_AddChildEntity_ChildCreationEventEnvelopeShouldHaveParentAndChildIds()
        {
            // Arrange
            var arrangements = GetArrangements();
            arrangements.AddCreationEvent();
            var sut = await arrangements.Repository.Load(arrangements.AggregateSutId, true);
            string childEntityName = "ChildEntityName";


            // Act
            sut.AddChild(arrangements.ChildEntityA, childEntityName, arrangements.CorrelationId);
            await arrangements.Repository.Save(sut);

            // Assert
            var envelope = arrangements.Events.SingleOrDefault(x => x.Event is EntityAdded) as ParcelVisionEventEnvelope<EntityId, AggregateRootTestId, EntityAdded>;
            envelope.Should().NotBeNull();
            envelope.ParentId.Should().Be(arrangements.AggregateSutId);
            envelope.SourceId.Should().Be(arrangements.ChildEntityA);
            envelope.SourceEntityType.Should().Be<EntityTest>();
        }

        [Fact]
        public async Task AggregateSut_ReconstituteAggregateWithCreationAndChildCreatedEvent_ExpectAddChildEntityCalledOnAggregate()
        {
            // Arrange
            string childName = "childName";
            var arrangements = GetArrangements();
            arrangements.AddCreationEvent();
            arrangements.AddChildEvent(arrangements.ChildEntityB, childName);

            // Act
            var sut = await arrangements.Repository.Load(arrangements.AggregateSutId, true);

            // Assert
            sut.Entities.Count.Should().Be(1);
            EntityTest aggregateChildEntity;
            sut.Entities.TryGetValue(arrangements.ChildEntityB, out aggregateChildEntity).Should().BeTrue();
            aggregateChildEntity.Name.Should().Be(childName);
            aggregateChildEntity.Id.Should().Be(arrangements.ChildEntityB);
        }

        [Fact]
        public async Task AggregateSut_ReconstituteAggregateWithCreationAndChildCreatedEvent_ExpectAddChildEntityToHaveParentPropertySetCorrectly()
        {
            // Arrange
            string childName = "childName";
            var arrangements = GetArrangements();
            arrangements.AddCreationEvent();
            arrangements.AddChildEvent(arrangements.ChildEntityB, childName);

            // Act
            var sut = await arrangements.Repository.Load(arrangements.AggregateSutId, true);

            // Assert
            sut.Entities.Count.Should().Be(1);
            EntityTest aggregateChildEntity;
            sut.Entities.TryGetValue(arrangements.ChildEntityB, out aggregateChildEntity).Should().BeTrue();
            aggregateChildEntity.Parent.Should().Be(sut);
        }

        [Fact]
        public async Task AggregateSut_AddChildEntityWhereExistsAlready_ExpectException()
        {
            // Arrange
            string childName = "childName";
            String newChildName = "newChildName";
            var arrangements = GetArrangements();
            arrangements.AddCreationEvent();
            arrangements.AddChildEvent(arrangements.ChildEntityA, childName);
            var sut = await arrangements.Repository.Load(arrangements.AggregateSutId, true);

            // Act
            var ex = Record.Exception(() => sut.AddChild(arrangements.ChildEntityA, newChildName, Guid.NewGuid()));
            await arrangements.Repository.Save(sut);


            // Assert
            ex.Should().NotBeNull();
            ex.Should().BeOfType<EntityExistsException>();
            ex.Message.Should()
                .Be(
                    $"Entity EntityTest with Id {arrangements.ChildEntityA} already exists within aggregate root of Id {arrangements.AggregateSutId}");
        }

        [Fact]
        public async Task AggregateSut_UpdateChildEntity_ExpectUpdatedEvent()
        {
            // Arrange
            string oldChildName = "childName", newName = "NewChildName";
            var arrangements = GetArrangements();
            arrangements.AddCreationEvent();
            arrangements.AddChildEvent(arrangements.ChildEntityA, oldChildName);
            var sut = await arrangements.Repository.Load(arrangements.AggregateSutId, true);

            // Act
            sut.UpdateChild(arrangements.ChildEntityA, newName, Guid.NewGuid());
            await arrangements.Repository.Save(sut);

            // Assert
            var @event = arrangements.Events.SingleOrDefault(x => x.Event is EntityUpdated).Event as EntityUpdated;
            @event.Should().NotBeNull();
            @event.EntityId.Should().Be(arrangements.ChildEntityA);
            @event.Name.Should().Be(newName);
        }

        [Fact]
        public async Task AggregateSut_UpdateChildEntity_ExpectChildEntityUpdated()
        {
            // Arrange
            string oldChildName = "childName", newName = "newChildName";
            var arrangements = GetArrangements();
            arrangements.AddCreationEvent();
            arrangements.AddChildEvent(arrangements.ChildEntityA, oldChildName);
            var sut = await arrangements.Repository.Load(arrangements.AggregateSutId, true);

            // Act
            sut.UpdateChild(arrangements.ChildEntityA, newName, Guid.NewGuid());
            await arrangements.Repository.Save(sut);

            // Assert
            sut.Entities.Count.Should().Be(1);
            sut.Entities.ContainsKey(arrangements.ChildEntityA).Should().BeTrue();
            var entity = sut.Entities[arrangements.ChildEntityA];
            entity.Name.Should().Be(newName);
        }

        [Fact]
        public async Task AggregateSut_AddChildSubEntity_ExpectAddedEvent()
        {
            // Arrange
            string childName = "childName", subChildName = "subChildName";
            var arrangements = GetArrangements();
            arrangements.AddCreationEvent();
            arrangements.AddChildEvent(arrangements.ChildEntityA, childName);
            var sut = await arrangements.Repository.Load(arrangements.AggregateSutId, true);

            // Act
            sut.AddSubChild(arrangements.ChildEntityA, arrangements.ChildSubEntityAA, subChildName, Guid.NewGuid());
            await arrangements.Repository.Save(sut);

            // Assert
            var @event = arrangements.Events.SingleOrDefault(x => x.Event is SubEntityCreated)?.Event as SubEntityCreated;
            @event.Should().NotBeNull();
            @event.Name.Should().Be(subChildName);
            @event.SubEntityId.Should().Be(arrangements.ChildSubEntityAA);
        }

        [Fact]
        public async Task AggregateSut_AddChildSubEntity_ExpectSubChildEntityToBeDiscoverableInAggregate()
        {
            // Arrange
            string childName = "childName", subChildName = "subChildName";
            var arrangements = GetArrangements();
            arrangements.AddCreationEvent();
            arrangements.AddChildEvent(arrangements.ChildEntityA, childName);
            var aggregate = await arrangements.Repository.Load(arrangements.AggregateSutId, true);

            // Act
            aggregate.AddSubChild(arrangements.ChildEntityA, arrangements.ChildSubEntityAA, subChildName, Guid.NewGuid());
            await arrangements.Repository.Save(aggregate);
            var subChild = aggregate.GetAggregateEntity<SubEntityId, SubChildEntityTest>(arrangements.ChildSubEntityAA);

            //Assert
            subChild.Should().NotBeNull();
            subChild.Id.Should().Be(arrangements.ChildSubEntityAA);
        }

        [Fact]
        public async Task AggregateSut_AddChildSubEntity_ExpectSubChildEntityAddedToChild()
        {
            // Arrange
            string childName = "childName", subChildName = "subChildName";
            var arrangements = GetArrangements();
            arrangements.AddCreationEvent();
            arrangements.AddChildEvent(arrangements.ChildEntityA, childName);
            var aggregate = await arrangements.Repository.Load(arrangements.AggregateSutId, true);

            // Act
            aggregate.AddSubChild(arrangements.ChildEntityA, arrangements.ChildSubEntityAA, subChildName, Guid.NewGuid());
            await arrangements.Repository.Save(aggregate);
            //We already validated the below on other tests.
            var child = aggregate.Entities[arrangements.ChildEntityA];

            // Assert
            child.SubEntities.Should().ContainKey(arrangements.ChildSubEntityAA);
            child.SubEntities[arrangements.ChildSubEntityAA].Name.Should().Be(subChildName);
            child.SubEntities[arrangements.ChildSubEntityAA].Id.Should().Be(arrangements.ChildSubEntityAA);
        }

        [Fact]
        public async Task AggregateSut_AddChildSubEntity_ExpectSubChildEntityToHaveParentSet()
        {
            // Arrange
            string childName = "childName", subChildName = "subChildName";
            var arrangements = GetArrangements();
            arrangements.AddCreationEvent();
            arrangements.AddChildEvent(arrangements.ChildEntityA, childName);
            var aggregate = await arrangements.Repository.Load(arrangements.AggregateSutId, true);

            // Act
            aggregate.AddSubChild(arrangements.ChildEntityA, arrangements.ChildSubEntityAA, subChildName, Guid.NewGuid());
            await arrangements.Repository.Save(aggregate);
            var subChild = aggregate.GetAggregateEntity<SubEntityId, SubChildEntityTest>(arrangements.ChildSubEntityAA);
            var child = aggregate.GetAggregateEntity<EntityId, EntityTest>(arrangements.ChildEntityA);

            //Assert
            subChild.Parent.Should().NotBeNull();
            subChild.Parent.Should().Be(child);
        }

        [Fact]
        public async Task AggregateSut_AddChildSubEntityWhenOneWithSameIdExists_ExpectException()
        {
            // Arrange
            string childName = "childName", subChildName = "subChildName";
            var arrangements = GetArrangements();
            arrangements.AddCreationEvent();
            arrangements.AddChildEvent(arrangements.ChildEntityA, childName);
            arrangements.AddSubChildCreatedEvent(arrangements.ChildSubEntityAA, arrangements.ChildEntityA, subChildName);
            var aggregate = await arrangements.Repository.Load(arrangements.AggregateSutId, true);

            // Act
            var exception = Record.Exception(() => aggregate.AddSubChild(arrangements.ChildEntityA, arrangements.ChildSubEntityAA, "NewName", Guid.NewGuid()));

            //Assert
            exception.Should().NotBeNull();
            exception.Should().BeOfType<EntityExistsException>();
            exception.As<EntityExistsException>().EntityId.Should().Be(arrangements.ChildSubEntityAA.ToString());
            exception.As<EntityExistsException>().EntityType.Should().Be(typeof(SubChildEntityTest));
        }

        [Fact]
        public async Task AggregateSut_ReconstituteAggregateWithChildSubEntityCreatedEvents_ExpectSubChildEntityToBeDiscoverableInAggregate()
        {
            // Arrange
            string childName = "childName", subChildName = "subChildName";
            var arrangements = GetArrangements();
            arrangements.AddCreationEvent();
            arrangements.AddChildEvent(arrangements.ChildEntityA, childName);
            arrangements.AddSubChildCreatedEvent(arrangements.ChildSubEntityAA, arrangements.ChildEntityA, subChildName);
            var aggregate = await arrangements.Repository.Load(arrangements.AggregateSutId, true);

            // Act
            var subChild = aggregate.GetAggregateEntity<SubEntityId, SubChildEntityTest>(arrangements.ChildSubEntityAA);

            //Assert
            subChild.Should().NotBeNull();
            subChild.Name.Should().Be(subChildName);
            subChild.Id.Should().Be(arrangements.ChildSubEntityAA);
        }

        [Fact]
        public async Task AggregateSut_ReconstituteAggregateWithChildSubEntityCreatedEvents_ExpectSubChildEntityToExistInParent()
        {
            string childName = "childName", subChildName = "subChildName";
            var arrangements = GetArrangements();
            arrangements.AddCreationEvent();
            arrangements.AddChildEvent(arrangements.ChildEntityA, childName);
            arrangements.AddSubChildCreatedEvent(arrangements.ChildSubEntityAA, arrangements.ChildEntityA, subChildName);
            var aggregate = await arrangements.Repository.Load(arrangements.AggregateSutId, true);

            // Act
            var child = aggregate.GetAggregateEntity<EntityId, EntityTest>(arrangements.ChildEntityA);

            //Assert
            //We assert that child should already not be null in other tests.
            child.SubEntities.Should().ContainKey(arrangements.ChildSubEntityAA);
            child.SubEntities[arrangements.ChildSubEntityAA].Id.Should().Be(arrangements.ChildSubEntityAA);
            child.SubEntities[arrangements.ChildSubEntityAA].Name.Should().Be(subChildName);
        }

        [Fact]
        public async Task AggregateSut_ReconstituteAggregateWithChildSubEntityCreatedEvents_ExpectSubChildEntityToHaveParentSet()
        {
            string childName = "childName", subChildName = "subChildName";
            var arrangements = GetArrangements();
            arrangements.AddCreationEvent();
            arrangements.AddChildEvent(arrangements.ChildEntityA, childName);
            arrangements.AddSubChildCreatedEvent(arrangements.ChildSubEntityAA, arrangements.ChildEntityA, subChildName);
            var aggregate = await arrangements.Repository.Load(arrangements.AggregateSutId, true);
            var child = aggregate.GetAggregateEntity<EntityId, EntityTest>(arrangements.ChildEntityA);

            // Act
            var subChild = aggregate.GetAggregateEntity<SubEntityId, SubChildEntityTest>(arrangements.ChildSubEntityAA);

            //Assert
            subChild.Parent.Should().NotBeNull();
            subChild.Parent.Should().Be(child);
        }

        [Fact]
        public async Task AggregateSut_UpdateSubChild_ExpectSubChildUpdatedEvent()
        {
            string childName = "childName", subChildName = "oldSubChild Name", newSubChildName = "newSubChildName";
            var arrangements = GetArrangements();
            arrangements.AddCreationEvent();
            arrangements.AddChildEvent(arrangements.ChildEntityA, childName);
            arrangements.AddSubChildCreatedEvent(arrangements.ChildSubEntityAA, arrangements.ChildEntityA, subChildName);
            var aggregate = await arrangements.Repository.Load(arrangements.AggregateSutId, true);

            // Act
            aggregate.UpdateSubChild(arrangements.ChildSubEntityAA, newSubChildName, Guid.NewGuid());
            await arrangements.Repository.Save(aggregate);

            //Assert
            var @event = arrangements.Events.SingleOrDefault(x => x.Event is SubEntityUpdated)?.Event as SubEntityUpdated;
            @event.Should().NotBeNull();
            @event.Name.Should().Be(newSubChildName);
            @event.SubEntityId.Should().Be(arrangements.ChildSubEntityAA);
        }

        //[Fact]
        //public async Task AggregateSut_AddChildSubEntityWhereExistsAlready_ExpectNoEvent()
        //{
        //    // Arrange
        //    var arrangements = GetArrangements();
        //    arrangements.AddCreationEvent();
        //    arrangements.AddChildEvent();
        //    arrangements.AddChildSubEvent();

        //    // Act
        //    var sut = await arrangements.Repository.Load(arrangements.AggregateSutId, true);
        //    var ex = Record.Exception(() => sut.AddSubChild(arrangements.ChildEntityA, arrangements.ChildSubEntityAA, "New Sub Entity Name",
        //        arrangements.CorrelationId));


        //    // Assert
        //    ex.Should().BeOfType<EntityExistsException>();
        //    ex.Message.Should()
        //        .Be(
        //            $"Entity SubEntityTest with Id {arrangements.ChildSubEntityAA} already exists within aggregate root of Id {arrangements.AggregateSutId}");
        //}


        ////[Fact]
        ////public async Task AggregateSut_RemoveChildSubEntity_ExpectRemovedEvent()
        ////{
        ////    // Arrange
        ////    var arrangements = GetArrangements();
        ////    arrangements.AddCreationEvent();
        ////    arrangements.AddChildEvent();
        ////    arrangements.AddChildSubEvent();

        ////    // Act
        ////    var sut = await arrangements.Repository.Load(arrangements.AggregateSutId, true);
        ////    var entity = sut.Entities.FirstOrDefault(x => arrangements.ChildEntityA.ToString() == x.Id.ToString());
        ////    entity.RemoveChild(arrangements.ChildSubEntityAA, arrangements.CorrelationId);
        ////    await arrangements.Repository.Save(sut);

        ////    // Assert
        ////    arrangements.Events.Count.Should().Be(4);

        ////    var envelope = arrangements.Events.FirstOrDefault(x => x.Event is SubEntityRemoved);
        ////    envelope.Should().NotBeNull();
        ////    envelope.SourceId.Should().Be(arrangements.ChildSubEntityAA);
        ////    envelope.ParentId.Should().Be(arrangements.ChildEntityA);
        ////    envelope.EntityType.Should().Be(typeof(SubEntitySut).AssemblyQualifiedName);

        ////    var @event = envelope.Event as SubEntityRemoved;
        ////    @event.AggregateSutId.Should().Be(arrangements.AggregateSutId);
        ////    @event.EntityId.Should().Be(arrangements.ChildSubEntityAA);
        ////    @event.CorrelationId.Should().Be(arrangements.CorrelationId);
        ////}

        ////[Fact]
        ////public async Task AggregateSut_RemoveChildSubEntity_ExpectChildSubEntityRemoved()
        ////{
        ////    // Arrange
        ////    var arrangements = GetArrangements();
        ////    arrangements.AddCreationEvent();
        ////    arrangements.AddChildEvent();
        ////    arrangements.AddChildSubEvent();

        ////    // Act
        ////    var sut = await arrangements.Repository.Load(arrangements.AggregateSutId, true);
        ////    var entity = sut.Entities.FirstOrDefault(x => arrangements.ChildEntityA.ToString() == x.Id.ToString());
        ////    entity.RemoveChild(arrangements.ChildSubEntityAA, arrangements.CorrelationId);
        ////    await arrangements.Repository.Save(sut);

        ////    // Assert
        ////    entity.SubEntities.Count().Should().Be(0);
        ////}

        ////[Fact]
        ////public async Task AggregateSut_RemoveUnknownChildSubEntity_ExpectNoEvent()
        ////{
        ////    // Arrange
        ////    var arrangements = GetArrangements();
        ////    arrangements.AddCreationEvent();
        ////    arrangements.AddChildEvent();
        ////    arrangements.AddChildSubEvent();

        ////    // Act
        ////    var sut = await arrangements.Repository.Load(arrangements.AggregateSutId, true);
        ////    var entity = sut.Entities.FirstOrDefault(x => arrangements.ChildEntityA.ToString() == x.Id.ToString());
        ////    entity.RemoveChild(arrangements.ChildSubEntityAB, arrangements.CorrelationId);
        ////    await arrangements.Repository.Save(sut);

        ////    // Assert
        ////    arrangements.Events.Count.Should().Be(3);
        ////}

        ////[Fact]
        ////public async Task AggregateSut_RemoveUnknownChildSubEntity_ExpecNoRemoval()
        ////{
        ////    // Arrange
        ////    var arrangements = GetArrangements();
        ////    arrangements.AddCreationEvent();
        ////    arrangements.AddChildEvent();
        ////    arrangements.AddChildSubEvent();

        ////    // Act
        ////    var sut = await arrangements.Repository.Load(arrangements.AggregateSutId, true);
        ////    var entity = sut.Entities.FirstOrDefault(x => arrangements.ChildEntityA.ToString() == x.Id.ToString());
        ////    entity.RemoveChild(arrangements.ChildSubEntityAB, arrangements.CorrelationId);
        ////    await arrangements.Repository.Save(sut);

        ////    // Assert
        ////    entity.SubEntities.Count().Should().Be(1);
        ////}

        //[Fact]
        //public async Task AggregateSut_UpdateChildSubEntity_ExpectUpdatedEvent()
        //{
        //    // Arrange
        //    var arrangements = GetArrangements();
        //    arrangements.AddCreationEvent();
        //    arrangements.AddChildEvent();
        //    arrangements.AddChildSubEvent();

        //    // Act
        //    var sut = await arrangements.Repository.Load(arrangements.AggregateSutId, true);
        //    sut.UpdateSubChild(arrangements.ChildSubEntityAA, "Dr No",
        //        arrangements.CorrelationId);
        //    await arrangements.Repository.Save(sut);

        //    // Assert
        //    arrangements.Events.Count.Should().Be(4);

        //    var envelope = arrangements.Events.FirstOrDefault(x => x.Event is SubEntityUpdated);
        //    envelope.Should().NotBeNull();
        //    //envelope.SourceId.Should().Be(arrangements.ChildSubEntityAA);
        //    //envelope.ParentId.Should().Be(arrangements.ChildEntityA);
        //    envelope.SourceEntityType.Should().Be(typeof(SubChildEntityTest).AssemblyQualifiedName);

        //    var @event = envelope.Event as SubEntityUpdated;
        //    @event.AggregateSutId.Should().Be(arrangements.AggregateSutId);
        //    @event.CorrelationId.Should().Be(arrangements.CorrelationId);
        //    @event.EntityId.Should().Be(arrangements.ChildSubEntityAA);
        //    @event.Name.Should().Be("Dr No");
        //}

        //[Fact]
        //public async Task AggregateSut_UpdateChildSubEntity_ExpectChildEntityUpdated()
        //{
        //    // Arrange
        //    var arrangements = GetArrangements();
        //    arrangements.AddCreationEvent();
        //    arrangements.AddChildEvent();
        //    arrangements.AddChildSubEvent();

        //    // Act
        //    var sut = await arrangements.Repository.Load(arrangements.AggregateSutId, true);
        //    sut.UpdateSubChild(arrangements.ChildSubEntityAA, "Dr No",
        //        arrangements.CorrelationId);
        //    await arrangements.Repository.Save(sut);

        //    // Assert
        //    var entity = sut.Entities.FirstOrDefault(x => arrangements.ChildEntityA.ToString() == x.Id.ToString());            
        //    entity.SubEntities.Count().Should().Be(1);

        //    var subEntity = entity.SubEntities.FirstOrDefault(x => arrangements.ChildSubEntityAA.ToString() == x.Id.ToString());
        //    subEntity.Name.Should().Be("Dr No");
        //}


        //[Fact]
        //public async Task AggregateSut_LoadFromRepositoryWithCreateAggregate_ExpectAggregateCreated()
        //{
        //    // Arrange
        //    var arrangements = GetArrangements();
        //    arrangements.AddCreationEvent();

        //    // Act
        //    var sut = await arrangements.Repository.Load(arrangements.AggregateSutId, true);

        //    // Assert
        //    sut.Id.Should().Be(arrangements.AggregateSutId);
        //    sut.Name.Should().Be("ABC123");
        //    sut.ConcurrencyId.Should().Be(1);
        //    sut.Entities.Count().Should().Be(0);          
        //}

        //[Fact]
        //public async Task AggregateSut_LoadFromRepositoryWithAddedChildEntity_ExpectChildEntityAdded()
        //{
        //    // Arrange
        //    var arrangements = GetArrangements();
        //    arrangements.AddCreationEvent();
        //    arrangements.AddChildEvent();

        //    // Act
        //    var sut = await arrangements.Repository.Load(arrangements.AggregateSutId, true);

        //    // Assert
        //    sut.Entities.Count().Should().Be(1);
        //    sut.ConcurrencyId.Should().Be(2);
        //    var entity = sut.Entities.FirstOrDefault(x => arrangements.ChildEntityA.ToString() == x.Id.ToString());
        //    entity.Should().NotBeNull();
        //    entity.Name.Should().Be("My name is Bond");
        //}

        //[Fact]
        //public async Task AggregateSut_LoadFromRepositoryWithUpdatedChildEntity_ExpectChildEntityUpdated()
        //{
        //    // Arrange
        //    var arrangements = GetArrangements();
        //    arrangements.AddCreationEvent();
        //    arrangements.AddChildEvent();
        //    arrangements.UpdateChildEvent();

        //    // Act
        //    var sut = await arrangements.Repository.Load(arrangements.AggregateSutId, true);

        //    // Assert
        //    sut.Entities.Count().Should().Be(1);
        //    sut.ConcurrencyId.Should().Be(3);
        //    var entity = sut.Entities.FirstOrDefault(x => arrangements.ChildEntityA.ToString() == x.Id.ToString());
        //    entity.Should().NotBeNull();
        //    entity.Name.Should().Be("James Bond");
        //}

        ////[Fact]
        ////public async Task AggregateSut_LoadFromRepositoryWithRemovedChildEntity_ExpectChildEntityRemoved()
        ////{
        ////    // Arrange
        ////    var arrangements = GetArrangements();
        ////    arrangements.AddCreationEvent();
        ////    arrangements.AddChildEvent();
        ////    arrangements.UpdateChildEvent();
        ////    arrangements.RemoveChildEvent();

        ////    // Act
        ////    var sut = await arrangements.Repository.Load(arrangements.AggregateSutId, true);

        ////    // Assert
        ////    sut.Entities.Count().Should().Be(0);
        ////    sut.ConcurrencyId.Should().Be(4);
        ////    var entity = sut.Entities.FirstOrDefault(x => x.Id == (IId)arrangements.ChildEntityA);
        ////    entity.Should().BeNull();
        ////}

        //[Fact]
        //public async Task AggregateSut_LoadFromRepositoryWithAddedChildSubEntity_ExpectChildSubEntityAdded()
        //{
        //    // Arrange
        //    var arrangements = GetArrangements();
        //    arrangements.AddCreationEvent();
        //    arrangements.AddChildEvent();
        //    arrangements.AddChildSubEvent();

        //    // Act
        //    var sut = await arrangements.Repository.Load(arrangements.AggregateSutId, true);

        //    // Assert
        //    var entity = sut.Entities.FirstOrDefault(x => arrangements.ChildEntityA.ToString() == x.Id.ToString());
        //    entity.SubEntities.Count().Should().Be(1);
        //    sut.ConcurrencyId.Should().Be(3);
        //    var subEntity = entity.SubEntities.FirstOrDefault(x => arrangements.ChildSubEntityAA.ToString() == x.Id.ToString());
        //    subEntity.Should().NotBeNull();
        //    subEntity.Name.Should().Be("My name is Q");
        //}

        //[Fact]
        //public async Task AggregateSut_LoadFromRepositoryWithUpdatedChildSubEntity_ExpectChildEntityUpdated()
        //{
        //    // Arrange
        //    var arrangements = GetArrangements();
        //    arrangements.AddCreationEvent();
        //    arrangements.AddChildEvent();
        //    arrangements.AddChildSubEvent();
        //    arrangements.UpdateChildSubEvent();

        //    // Act
        //    var sut = await arrangements.Repository.Load(arrangements.AggregateSutId, true);

        //    // Assert
        //    var entity = sut.Entities.FirstOrDefault(x => arrangements.ChildEntityA.ToString() == x.Id.ToString());
        //    entity.SubEntities.Count().Should().Be(1);
        //    sut.ConcurrencyId.Should().Be(4);
        //    var subEntity = entity.SubEntities.FirstOrDefault(x => arrangements.ChildSubEntityAA.ToString() == x.Id.ToString());
        //    subEntity.Should().NotBeNull();
        //    subEntity.Name.Should().Be("Jaws");
        //}

        ////[Fact]
        ////public async Task AggregateSut_LoadFromRepositoryWithRemovedChildSubEntity_ExpectChildEntityRemoved()
        ////{
        ////    // Arrange
        ////    var arrangements = GetArrangements();
        ////    arrangements.AddCreationEvent();
        ////    arrangements.AddChildEvent();
        ////    arrangements.AddChildSubEvent();
        ////    arrangements.RemoveChildSubEvent();

        ////    // Act
        ////    var sut = await arrangements.Repository.Load(arrangements.AggregateSutId, true);

        ////    // Assert
        ////    var entity = sut.Entities.FirstOrDefault(x => x.Id.ToString() == arrangements.ChildEntityA.ToString());
        ////    entity.SubEntities.Count().Should().Be(0);
        ////    sut.ConcurrencyId.Should().Be(4);
        ////    var subEntity = entity.SubEntities.FirstOrDefault(x => x.Id.ToString() == arrangements.ChildSubEntityAA.ToString());
        ////    subEntity.Should().BeNull();
        ////}

        //[Fact]
        //public void AggregateSut_LoadFromRepositoryWithNotImplementedAggregateEvent_ExpectException()
        //{
        //    // Arrange
        //    var arrangements = GetArrangements();
        //    arrangements.AddCreationEvent();
        //    arrangements.AddNotImplementedAggregateEvent();

        //    // Act
        //    Func<Task> sut = async () => await arrangements.Repository.Load(arrangements.AggregateSutId, true);

        //    // Assert
        //    sut.ShouldThrow<EventNotSupportedException>()
        //        .WithMessage($"Entity AggregateRootTest with Id {arrangements.AggregateSutId} could not process event NotImplementedAggregateEvent");
        //}

        //[Fact]
        //public void AggregateSut_LoadFromRepositoryWithNotImplementedEntityEvent_ExpectException()
        //{
        //    // Arrange
        //    var arrangements = GetArrangements();
        //    arrangements.AddCreationEvent();
        //    arrangements.AddChildEvent();
        //    arrangements.AddNotImplementedEntityEvent();

        //    // Act
        //    Func<Task> sut = async () => await arrangements.Repository.Load(arrangements.AggregateSutId, true);

        //    // Assert
        //    sut.ShouldThrow<EventNotSupportedException>()
        //        .WithMessage($"Entity EntityTest with Id {arrangements.ChildEntityA} could not process event NotImplementedEntityEvent");
        //}

        ////[Fact]
        ////public async Task AggregateSut_LoadFromRepositoryWhereCreatedEventIsMissing_IdShouldBeNull()
        ////{
        ////    // Arrange
        ////    var arrangements = GetArrangements();
        ////    arrangements.AddUpdatedEvent();

        ////    // Act
        ////    var sut = await arrangements.Repository.Load(arrangements.AggregateSutId, true);

        ////    // Assert
        ////    sut.Id.Should().BeNull();
        ////}

        //[Fact]
        //public async Task AggregateSut_ChildEntityOperationWithoutInitializingEntityId_ExpectException()
        //{
        //    // Arrange
        //    var arrangements = GetArrangements();
        //    arrangements.AddCreationEvent();
        //    var sut = await arrangements.Repository.Load(arrangements.AggregateSutId, true);

        //    // Act
        //    var ex = Record.Exception(() => sut.AddChildWithoutId("ID-less", arrangements.CorrelationId));

        //    // Assert
        //    ex.Should().BeOfType<EntityWithoutIdException>();
        //    ex.Message.Should().Be("Entity EntityTest is without an Id");
        //}

        [Fact]
        public async Task Repository_LoadUnknownAggregateAndConfiguredNotToThrow_ExpectNoThrow()
        {
            // Arrange
            var arrangements = GetArrangements();

            // Act
            var sut = await arrangements.Repository.Load(arrangements.AggregateSutId, false);

            // Assert
            sut.Should().BeNull();
        }

        [Fact]
        public void Repository_LoadUnknownAggregateAndConfiguredToThrow_ExpectException()
        {
            // Arrange
            var arrangements = GetArrangements();

            // Act
            Func<Task> sut = async () => await arrangements.Repository.Load(arrangements.AggregateSutId, true);

            // Assert
            sut.ShouldThrow<AggregateNotFoundException>()
                .WithMessage($"Aggregate AggregateRootTest with id {arrangements.AggregateSutId} was not found");
        }
    }
}