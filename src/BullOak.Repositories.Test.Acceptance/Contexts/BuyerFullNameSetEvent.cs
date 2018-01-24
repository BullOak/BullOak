namespace BullOak.Repositories.Test.Acceptance.Contexts
{
    public class BuyerFullNameSetEvent
    {
        public string FullName { get; set; }

        public BuyerFullNameSetEvent(string fullname)
            => FullName = fullname;
    }
}