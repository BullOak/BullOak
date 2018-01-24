using BullOak.Repositories.EventStore.Test.Integration.Components;
using System;

namespace BullOak.Repositories.EventStore.Test.Integration.Contexts
{
    internal class TestDataContext
    {
        internal Guid CurrentStreamId { get; set; } = Guid.NewGuid();
        public Exception RecordedException { get; internal set; }
        public IHoldHigherOrder LatestLoadedState { get; internal set; }

        internal MyEvent[] LastGeneratedEvents;

        internal void ResetStream()
        {
            CurrentStreamId = Guid.NewGuid();
        }


    }
}
