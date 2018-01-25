﻿

namespace BullOak.Repositories.EventStore.Test.Integration.Contexts
{
    using BullOak.Repositories.EventStore.Test.Integration.Components;
    using BullOak.Repositories.Session;
    using System;
    using System.Collections.Generic;
    internal class TestDataContext
    {
        internal Guid CurrentStreamId { get; set; } = Guid.NewGuid();
        public Exception RecordedException { get; internal set; }
        public IHoldHigherOrder LatestLoadedState { get; internal set; }
        public Dictionary<string, IManageAndSaveSessionWithSnapshot<IHoldHigherOrder>> NamedSessions { get; internal set; } =
            new Dictionary<string, IManageAndSaveSessionWithSnapshot<IHoldHigherOrder>>();
        public Dictionary<string, Exception> NamedSessionsExceptions { get; internal set; } = new Dictionary<string, Exception>();

        internal MyEvent[] LastGeneratedEvents;

        internal void ResetStream()
        {
            CurrentStreamId = Guid.NewGuid();
        }


    }
}
