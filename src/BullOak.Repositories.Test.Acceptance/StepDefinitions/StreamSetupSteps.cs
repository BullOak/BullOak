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
            sessionContainer.SaveStream(streamInfo.Id, new (ItemWithType, DateTime)[0]);
        }

        [Given(@"a stream with (.*) events?")]
        [Given(@"an existing stream with (.*) events?")]
        public void GivenAnExistingStreamWithEvents(int eventCount)
        {
            var events = eventGenerator.GenerateEvents(eventCount);
            var originalEvents = sessionContainer.GetStream(streamInfo.Id);

            var combined = new List<(ItemWithType,DateTime)>(originalEvents);
            combined.AddRange(events.Select(x=> (new ItemWithType(x), DateTime.UtcNow)));

            sessionContainer.SaveStream(streamInfo.Id, combined.ToArray());
        }

        [Given(@"an existing stream with (.*) events with timestamps")]
        public void GivenAnExistingStreamWithEventsWithTimestamps(int eventCount, Table timestamps)
        {
            var events = eventGenerator.GenerateEvents(eventCount);
            var originalEvents = sessionContainer.GetStream(streamInfo.Id);

            var combined = new List<(ItemWithType, DateTime)>(originalEvents);
            int index = 0;
            foreach (var ev in events)
            {
                var datetime = DateTime.Parse(timestamps.Rows[index++].Values.ElementAt(0));
                combined.Add((new ItemWithType(ev), datetime));
            }

            sessionContainer.SaveStream(streamInfo.Id, combined.ToArray());
        }


        [Given(@"a buyer name set event")]
        [Given(@"a buyer name set event which can be upconverted as below in the stream")]
        public void GivenABuyerNameSetEventWhichCanBeUpconvertedAsBelowInTheStream(Table table)
        {
            var nameInfo = table.CreateInstance<BuyerNameSetEvent>();

            var @event = new BuyerFullNameSetEvent()
            {
                FullName = $"{nameInfo.Title} {nameInfo.Name} {nameInfo.Surname}"
            };
            var originalEvents = sessionContainer.GetStream(streamInfo.Id);
            var combined = new List<(ItemWithType, DateTime)>(originalEvents);
            combined.Add((new ItemWithType(@event), DateTime.UtcNow));

            sessionContainer.SaveStream(streamInfo.Id, combined.ToArray());
        }

        [Given(@"a full buyer name set event")]
        public void GivenAFullBuyerNameSetEvent(Table table)
        {
            var @event = table.CreateInstance<BuyerNameSetEvent>();

            sessionContainer.SaveStream(streamInfo.Id, new[] {(new ItemWithType(@event), DateTime.UtcNow)});
        }

        [Given(@"a balance set event with balance (.*) and date (.*)")]
        public void GivenABalanceSetEventWithBalanceAndDate(Decimal balance, DateTime timestamp)
        {
            var @event = new BalanceUpdatedSetEvent()
            {
                Balance = balance,
                UpdatedDate = timestamp,
            };

            sessionContainer.SaveStream(streamInfo.Id, new [] {(new ItemWithType(@event), DateTime.UtcNow) });
        }
    }
}
