namespace BullOak.Repositories
{
    public class Configuration
    {
        public static IConfigureEventCollectionType Begin() => new ConfigurationOwner();
    }
}
