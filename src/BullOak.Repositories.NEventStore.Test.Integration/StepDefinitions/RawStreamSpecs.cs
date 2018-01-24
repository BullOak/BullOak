namespace BullOak.Repositories.NEventStore.Test.Integration.StepDefinitions
{
    using System;
    using System.Linq;
    using BullOak.Repositories.NEventStore.Test.Integration.Contexts;
    using FluentAssertions;
    using global::NEventStore;
    using TechTalk.SpecFlow;

    [Binding]
    internal class RawStreamSpecs
    {
        private EventGenerator eventGenerator;
        private StreamInfoContainer streamInfo;
        private NEventStoreContainer neventStoreContainer;

        public RawStreamSpecs(EventGenerator eventGenerator, StreamInfoContainer streamInfo, NEventStoreContainer neventStoreContainer)
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

        [Then(@"there should be (.*) events? in the stream")]
        public void ThenThereShouldBeEventInTheStream(int expectedEventCount)
        {
            using (var stream = neventStoreContainer.OpenStream(streamInfo.Id, streamInfo.Revision))
            {
                stream.CommittedEvents.Count.Should().Be(expectedEventCount);
            }
        }

        [Then(@"there should be (.*) events? in the stream with Order higher or equal to (.*)")]
        public void ThenThereShouldBeEventInTheStreamWithHighOrderOf(int expectedEventCount, int minimumOrder)
        {
            using (var stream = neventStoreContainer.OpenStream(streamInfo.Id, streamInfo.Revision))
            {
                stream.CommittedEvents
                    .Where(x => x.Body is MyEvent)
                    .Select(x => x.Body)
                    .Cast<MyEvent>()
                    .Where(x => x.Order >= minimumOrder)
                    .Count()
                    .Should().Be(expectedEventCount);
            }
        }
    }
}
