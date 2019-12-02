namespace BullOak.Repositories.Test.Acceptance.Contexts
{
    using BullOak.Repositories.Upconverting;

    public class BuyerNameUpconverter : IUpconvertEvent<BuyerNameSetEvent, BuyerFullNameSetEvent>
    {
        public BuyerFullNameSetEvent Upconvert(BuyerNameSetEvent source)
            => new BuyerFullNameSetEvent($"{source.Title} {source.Name} {source.Surname}");
    }
}
