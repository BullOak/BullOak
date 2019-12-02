namespace BullOak.Repositories.Test.Acceptance.Contexts
{
    public class BalanceSetEvent
    {
        public decimal CurrentBalance { get; set; }

        public BalanceSetEvent(decimal balance)
            => CurrentBalance = balance;
    }
}
