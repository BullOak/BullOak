namespace BullOak.Repositories.Test.Acceptance.Contexts
{
    using System.Collections.Generic;
    using BullOak.Repositories.Upconverting;

    public class BalanceUpconverter : IUpconvertEvent<BalanceUpdatedSetEvent>
    {
        public IEnumerable<object> Upconvert(BalanceUpdatedSetEvent source)
        {
            yield return new BalanceSetEvent(source.Balance);
            yield return new TimeOfLastBalanceUpdateSetEvent(source.UpdatedDate);
        }
    }
}