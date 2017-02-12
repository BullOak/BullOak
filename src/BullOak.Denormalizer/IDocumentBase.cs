namespace BullOak.Denormalizer
{
    public interface IDocumentBase
    {
        string ETag { get; set; }
    }

    public interface IDocumentBase<T> : IDocumentBase
    {
        T VM { get; set; }
    }
}