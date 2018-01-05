//namespace BullOak.Repositories.NEventStore
//{
//    using System;
//    using System.Collections.Generic;
//    using System.Linq;
//    using System.Threading.Tasks;
//    using BullOak.Common;
//    using BullOak.Repositories.Appliers;
//    using BullOak.Repositories.Session;
//    using CommonDomain;
//    using global::NEventStore;

//    internal class NEventStoreRepository<TState>: IManagePersistenceOf<string, NEventStoreSession<TState>, TState>
//    {
//        private readonly IStoreEvents store;
//        private readonly IHoldAllConfiguration configuration;

//        public NEventStoreRepository(IStoreEvents store, IHoldAllConfiguration configuration)
//        {
//            this.store = store ?? throw new ArgumentNullException(nameof(store));
//            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
//        }

//        public Task<NEventStoreSession<TState>> BeginSessionFor(string id, bool throwIfNotExists = false)
//        {
//            var stream = store.OpenStream(streamId: id);
//            var sn = new Snapshot("", 0, null);

//            store.OpenStream(sn, int.MaxValue);
//            var events = stream.CommittedEvents
//                .Select(x => x.Body)
//                .ToArray();

//            var session = new NEventStoreSession<TState>(configuration, stream);

//            return Task.FromResult(session);
//        }

//        public Task Clear(string id)
//        {
//            store.Advanced.DeleteStream("bucketId", id);
//            return Task.FromResult(true);
//        }

//        public Task<bool> Exists(string id)
//        {
//            try
//            {
//                var stream = store.OpenStream("bucketId", id, int.MinValue, int.MaxValue);
//                return Task.FromResult(stream.CommittedEvents.Count > 0);
//            }
//            catch (StreamNotFoundException)
//            {
//                return Task.FromResult(false);
//            }
//        }
//    }
//}
