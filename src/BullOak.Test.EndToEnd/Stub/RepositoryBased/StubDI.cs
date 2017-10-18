namespace BullOak.Test.EndToEnd.Stub.RepositoryBased
{
    using BullOak.Repositories;
    using BullOak.Test.EndToEnd.Stub.RepositoryBased.CinemaAggregate;
    using BullOak.Test.EndToEnd.Stub.RepositoryBased.ViewingAggregate;
    using BullOak.Test.EndToEnd.Stub.Shared.Messages;

    internal static class StubDI
    {
        public static ICreateEventAppliers GetCreator()
        {
            var container = new EventApplierContainer();

            container.Register(new CinemaCreatedReconstitutor());
            var viewingReconstitutor = new ViewingReconstitutor();
            container.Register(viewingReconstitutor);

            return container.Build();
        }
    }
}
