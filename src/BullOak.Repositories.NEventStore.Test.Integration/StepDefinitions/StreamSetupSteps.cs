namespace BullOak.Repositories.NEventStore.Test.Integration.StepDefinitions
{
    using System;
    using BullOak.Repositories.NEventStore.Test.Integration.Contexts;
    using global::NEventStore;
    using TechTalk.SpecFlow;

    [Binding]
    internal class StreamSetupSteps
    {
        private EventGenerator eventGenerator;
        private StreamInfoContainer streamInfo;
        private NEventStoreContainer neventStoreContainer;

        public StreamSetupSteps(EventGenerator eventGenerator, StreamInfoContainer streamInfo, NEventStoreContainer neventStoreContainer)
        {
            this.eventGenerator = eventGenerator;
            this.streamInfo = streamInfo;
            this.neventStoreContainer = neventStoreContainer;
        }

        [Given(@"a new stream")]
        public void GivenANewStream()
        {
            using (var stream = neventStoreContainer.OpenStream(streamInfo.Id, streamInfo.Revision))
            {
                stream.CommitChanges(Guid.NewGuid());
            }
        }

        [Given(@"a stream with (.*) events?")]
        [Given(@"an existing stream with (.*) events?")]
        public void GivenAnExistingStreamWithEvents(int eventCount)
        {
            using (var stream = neventStoreContainer.OpenStream(streamInfo.Id, streamInfo.Revision))
            {
                streamInfo.Revision = eventCount + stream.CommittedEvents.Count;

                if (eventCount > 0)
                {
                    foreach (var @event in eventGenerator.GenerateEvents(eventCount))
                    {
                        stream.Add(new EventMessage()
                        {
                            Body = @event
                        });
                    }
                }

                stream.CommitChanges(Guid.NewGuid());
            }
        }
    }
}
