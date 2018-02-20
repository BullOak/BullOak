namespace BullOak.Repositories
{
    using BullOak.Repositories.Session.CustomLinkedList;

    public static class EventCollectionConfigExtensions
    {
        // The below works fine, but it increases the surface area unnecessarilly, so its commented out.
        // If there is any need\requests it can be added in.
        //public static IConfigureStateFactory WithTypeOfEventCollection<TEventCollection>(
        //    this IConfigureEventCollectionType self)
        //    where TEventCollection : ICollection<object>, new()
        //    => self.WithEventCollectionSelector(_ => () => new TEventCollection());

        public static IConfigureStateFactory WithDefaultCollection(this IConfigureEventCollectionType self)
            => self.WithEventCollectionSelector(_ => () => new LinkedList<ItemWithType>());

    }
}
