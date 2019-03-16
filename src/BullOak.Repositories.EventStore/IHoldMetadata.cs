namespace BullOak.Repositories.EventStore
{
    public interface IHoldMetadata
    {
        int MetadataVersion { get; set; }
        string EventTypeFQN { get; set; }
    }
}