using BullOak.Repositories.EventStore.Test.Integration.Components;
using System;

namespace BullOak.Repositories.EventStore.Test.Integration.Contexts
{
    internal class TestDataContext
    {
        internal Guid CurrentStreamInUse { get; set; } = Guid.NewGuid();
        public Exception RecordedException { get; internal set; }

        internal MyEvent[] LastGeneratedEvents;

        internal void ResetStream()
        {
            CurrentStreamInUse = Guid.NewGuid();
        }


    }
}
