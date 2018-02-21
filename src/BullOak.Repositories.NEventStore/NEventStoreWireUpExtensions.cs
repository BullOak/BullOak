namespace BullOak.Repositories.NEventStore
{
    using global::NEventStore;

    public static class NEventStoreWireUpExtensions
    {
        public static SerializationWireup WithSerializationForInterfaceMessages(
            this PersistenceWireup persistenceWireup,
            IHoldAllConfiguration configuration)
            => persistenceWireup.UsingCustomSerialization(new CustomSerializer(configuration));
    }
}
