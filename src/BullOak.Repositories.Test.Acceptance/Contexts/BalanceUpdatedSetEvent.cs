namespace BullOak.Repositories.Test.Acceptance.Contexts
{
    using System;

    public class BalanceUpdatedSetEvent
    {
        public decimal Balance { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
