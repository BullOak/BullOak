namespace BullOak.Repositories.StateEmit
{
    using System;

    internal class StateController<TState> : IControlStateWritability<TState>
    {
        private readonly TState state;
        private readonly ICanSwitchBackAndToReadOnly writableSwitcher;

        public void MakeStateWritable() => writableSwitcher.CanEdit = true;
        public void MakeStateReadOnly() => writableSwitcher.CanEdit = false;
        public TState State => state;

        public StateController(TState state)
        {
            if (!(state is ICanSwitchBackAndToReadOnly)) throw new ArgumentException("Argument is not of correctType", nameof(state));

            this.state = state;
            writableSwitcher = (ICanSwitchBackAndToReadOnly)state;
        }
    }

    internal class DoNothingController<TState> : IControlStateWritability<TState>
    {
        public void MakeStateWritable() { }
        public void MakeStateReadOnly() { }

        private readonly TState state;
        public TState State => state;

        public DoNothingController(TState state) => this.state = state;
    }
}
