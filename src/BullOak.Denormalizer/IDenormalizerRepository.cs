namespace BullOak.Denormalizer
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IDenormalizerRepository<T>
    {
        Task<IEnumerable<T>> LoadAll();

        Task<DocumentBase<T>> GetById(string id);

        Task<IEnumerable<T>> GetByExample(object example);

        Task Delete(string id);

        Task Upsert(string id, DocumentBase<T> item);
    }
}
