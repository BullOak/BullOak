namespace BullOak.Repositories.NEventStore.Test.Integration.StepDefinitions
{
    using System;
    using System.Collections.Generic;
    using BullOak.Repositories.NEventStore.Test.Integration.Contexts;
    using TechTalk.SpecFlow;

    [Binding]
    internal class StreamSetupSteps
    {
        private EventGenerator eventGenerator;
        private StreamInfoContainer streamInfo;
        private InMemoryStoreSessionContainer sessionContainer;

        public StreamSetupSteps(EventGenerator eventGenerator, StreamInfoContainer streamInfo,
            InMemoryStoreSessionContainer sessionContainer)
        {
            this.eventGenerator = eventGenerator;
            this.streamInfo = streamInfo;
            this.sessionContainer = sessionContainer;
        }

        [Given(@"a new stream")]
        public void GivenANewStream()
        {
            sessionContainer.SaveStream(streamInfo.Id, new object[0]);
        }

        [Given(@"a stream with (.*) events?")]
        [Given(@"an existing stream with (.*) events?")]
        public void GivenAnExistingStreamWithEvents(int eventCount)
        {
            var events = eventGenerator.GenerateEvents(eventCount);
            var originalEvents = sessionContainer.GetStream(streamInfo.Id);

            var combined = new List<object>(originalEvents);
            combined.AddRange(events);

            sessionContainer.SaveStream(streamInfo.Id, combined.ToArray());
        }
    }
}
