namespace BullOak.Application
{
    using System;
    using EventStream;
    using System.Threading.Tasks;
    using BullOak.Messages;
    using Common;
    using Exceptions;

    public class AggregateRepositoryBase<TAggregateRoot, TId> : IAggregateRepository<TAggregateRoot, TId>
        where TId : IId, IEquatable<TId>
        where TAggregateRoot : AggregateRoot<TId>, new()
    {
        private readonly IEventStore eventStore;
        private readonly Action<string, TId, AggregateRoot<TId>> aggregateLoadLogger;
        private readonly Func<Guid, Action<TId, AggregateRoot<TId>>> aggregateCreatedLogger;
        private readonly Func<Guid, Action<TId, AggregateRoot<TId>>> aggregateUpdatedLogger;
        private readonly Func<TId, Action<string, IParcelVisionEvent>> verboseEventLogger;
        private readonly Func<TId, Action<string, IParcelVisionEvent, Guid>> eventRaisedLogger;
        private readonly Action<string, TId> aggregateNotFoundLogger;

        public AggregateRepositoryBase(IEventStore eventStore,
            Action<string, TId, AggregateRoot<TId>> aggregateLoadLogger = null,
            Func<Guid, Action<TId, AggregateRoot<TId>>> aggregateCreatedLogger = null,
            Func<Guid, Action<TId, AggregateRoot<TId>>> aggregateUpdatedLogger = null,
            Action<string, TId> aggregateNotFoundLogger = null,
            Func<TId, Action<string, IParcelVisionEvent>> verboseEventLogger = null,
            Func<TId, Action<string, IParcelVisionEvent, Guid>> eventRaisedLogger = null)
        {
            if (eventStore == null) throw new ArgumentNullException(nameof(eventStore));

            this.eventStore = eventStore;
            this.aggregateLoadLogger = aggregateLoadLogger;
            this.aggregateCreatedLogger = aggregateCreatedLogger;
            this.aggregateUpdatedLogger = aggregateUpdatedLogger;
            this.verboseEventLogger = verboseEventLogger;
            this.eventRaisedLogger = eventRaisedLogger;
            this.aggregateNotFoundLogger = aggregateNotFoundLogger;
        }

        public Task<bool> Exists(string id) => eventStore.Exists(id);

        public async Task<TAggregateRoot> Load(TId aggregateId, bool throwIfNotFound = true)
        {
            var data = await eventStore.LoadFor(aggregateId.ToString());

            var aggregateRoot = new TAggregateRoot()
            {
                verboseLoggingFunc = verboseEventLogger?.Invoke(aggregateId),
                eventRaiseLoggingFunc = eventRaisedLogger?.Invoke(aggregateId)
            };

            if (data.ConcurrencyId != 0)
            {
                aggregateRoot.ReconstituteAggregate(data.EventEnvelopes, data.ConcurrencyId);

                aggregateLoadLogger?.Invoke("Stream with {@id} reconstituted to {@aggregate}", aggregateId, aggregateRoot);

                return aggregateRoot;
            }

            if (throwIfNotFound)
            {
                aggregateNotFoundLogger?.Invoke("Aggregate with {@id} not found", aggregateId);

                throw new AggregateNotFoundException(aggregateId.ToString(), typeof(TAggregateRoot));
            }

            return null;
        }

        public async Task Save(TAggregateRoot aggregateRoot)
        {
            var aggregateStreamOwner = (IOwnAggregateEventStream) aggregateRoot;

            var newEvents = aggregateStreamOwner.GetUncommitedEventsForAggregate();
            var concurrencyId = aggregateRoot.ConcurrencyId;

            if (newEvents.Length == 0) return;

            await eventStore.Store(aggregateRoot.Id.ToString(), concurrencyId, newEvents);

            var cId = newEvents[0].Event.CorrelationId;

            if(concurrencyId == 0) aggregateCreatedLogger?.Invoke(cId)?.Invoke(aggregateRoot.Id, aggregateRoot);
            else aggregateUpdatedLogger?.Invoke(cId)?.Invoke(aggregateRoot.Id, aggregateRoot);

            aggregateStreamOwner.ClearUncommitedEvents();
        }
    }
}
