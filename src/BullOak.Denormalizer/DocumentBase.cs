namespace BullOak.Denormalizer
{
    public class DocumentBase<T> : IDocumentBase<T>
    {
        public string ETag { get; set; }
        public T VM { get; set; }

        public DocumentBase()
        {
            ETag = System.Guid.NewGuid().ToString();
        }
    }
}