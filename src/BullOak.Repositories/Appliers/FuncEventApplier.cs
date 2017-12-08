namespace BullOak.Repositories.Appliers
{
    using System;

    public class FuncEventApplier<TState, TEvent> : BaseApplyEvents<TState, TEvent>
    {
        private readonly Func<TState, TEvent, TState> applyFunc;

        public FuncEventApplier(Func<TState, TEvent, TState> applyFunc)
            => this.applyFunc = applyFunc ?? throw new ArgumentNullException(nameof(applyFunc));

        public override TState Apply(TState state, TEvent @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));

            return applyFunc(state, @event);
        }

        public static implicit operator FuncEventApplier<TState, TEvent>(Func<TState, TEvent, TState> handler)
            => new FuncEventApplier<TState, TEvent>(handler);
    }
}
