using BullOak.Repositories.Appliers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BullOak.Repositories.Test.Unit.Session
{
    internal class TestState
    {
        public int Count { get; set; }
        public string Name { get; set; }
    }

    internal class CountChangedClassEvent: ICountChangedInterfaceEvent
    {
        public int NewCount { get; set; }
    }

    public interface ICountChangedInterfaceEvent
    {
        int NewCount { get; set; }
    }

    internal class CountChangedApplier : IApplyEventsToStates
    {
        public IEnumerable<Type> SupportedStateTypes => new[] { typeof(TestState) };

        public TestState Apply(TestState state, CountChangedClassEvent @event)
        {
            state.Count = @event.NewCount;
            return state;
        }

        public TestState Apply(TestState state, ICountChangedInterfaceEvent @event)
        {
            state.Count = @event.NewCount;
            return state;
        }

        public ApplyResult Apply(Type stateType, object state, ItemWithType[] events)
            => new ApplyResult(events.Aggregate(state, (s, e) => ApplyEvent(stateType, s, e)), events.Length > 0);

        public ApplyResult Apply(Type stateType, object state, IEnumerable<ItemWithType> events)
            => Apply(stateType, state, events.ToArray());

        public async Task<ApplyResult> Apply(Type stateType, object state, IAsyncEnumerable<ItemWithType> events)
        {
            bool anyEvent = false;
            await foreach (var e in events)
            {
                anyEvent = true;
                state = ApplyEvent(stateType, state, e);
            }

            return new ApplyResult(state, anyEvent);
        }


        public object ApplyEvent(Type stateType, object state, ItemWithType @event)
        {
            if (@event.instance is CountChangedClassEvent countClassEvent)
            {
                return Apply(state as TestState, countClassEvent);
            }

            if (@event.instance is ICountChangedInterfaceEvent countInterfaceEvent)
            {
                return Apply(state as TestState, countInterfaceEvent);
            }

            throw new NotSupportedException();
        }
    }
}
