namespace BullOak.Repositories.Test.Acceptance.Contexts
{
    using System;

    public interface IHoldHigherOrder
    {
        int HigherOrder { get; set; }
        string FullName { get; set; }
        decimal LastBalance { get; set; }
        DateTime BalaceUpdateTime { get; set; }
    }
}
