namespace BullOak.Repositories.Test.Acceptance.StepDefinitions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BullOak.Repositories.Test.Acceptance.Contexts;
    using TechTalk.SpecFlow;
    using TechTalk.SpecFlow.Assist;

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
            sessionContainer.SaveStream(streamInfo.Id, new ItemWithType[0]);
        }

        [Given(@"a stream with (.*) events?")]
        [Given(@"an existing stream with (.*) events?")]
        public void GivenAnExistingStreamWithEvents(int eventCount)
        {
            var events = eventGenerator.GenerateEvents(eventCount);
            var originalEvents = sessionContainer.GetStream(streamInfo.Id);

            var combined = new List<ItemWithType>(originalEvents);
            combined.AddRange(events.Select(x=> new ItemWithType(x)));

            sessionContainer.SaveStream(streamInfo.Id, combined.ToArray());
        }

        [Given(@"a buyer name set event which can be upconverted as below in the stream")]
        public void GivenABuyerNameSetEventWhichCanBeUpconvertedAsBelowInTheStream(Table table)
        {
            var @event = table.CreateInstance<BuyerNameSetEvent>();

            sessionContainer.SaveStream(streamInfo.Id, new [] {new ItemWithType(@event) });
        }

        [Given(@"a balance set event with balance (.*) and date (.*)")]
        public void GivenABalanceSetEventWithBalanceAndDate(Decimal balance, DateTime timestamp)
        {
            var @event = new BalanceUpdatedSetEvent()
            {
                Balance = balance,
                UpdatedDate = timestamp,
            };

            sessionContainer.SaveStream(streamInfo.Id, new [] {new ItemWithType(@event) });
        }
    }
}
