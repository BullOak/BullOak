namespace BullOak.Denormalizer
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Linq;

    public abstract class DocumentDbRepository<T> : IDenormalizerRepository<T>
    {
        private readonly string collectionName;
        private readonly Lazy<Task<IDocumentStore>> db;

        protected abstract Action<string, T, string> LogRead { get; }
        protected abstract Action<string, IEnumerable<T>, string> LogReadMany { get; }
        protected abstract Action<string, T, string> LogSave { get; }
        protected abstract Action<string, string> LogDelete { get; }

        protected DocumentDbRepository(IDocumentStore db, string collectionName = null)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));

            this.collectionName = collectionName ?? typeof(T).FullName;

            this.db = new Lazy<Task<IDocumentStore>>(async () =>
            {
                await db.CreateCollection(this.collectionName);

                return db;
            });
        }

        public async Task Delete(string id)
        {
            var db = await this.db.Value;

            await db.DeleteDocument(collectionName, id);

            LogDelete("Deleted document with {documentKey}", id);
        }

        public async Task<DocumentBase<T>> GetById(string id)
        {
            var db = await this.db.Value;

            var doc = await db.ReadDocument<T>(collectionName, id);

            LogRead("Loaded {@vm} for {documentKey}", doc != null ? doc.VM : default(T), id);

            return doc;
        }

        public async Task<IEnumerable<T>> GetByExample(object example)
        {
            var db = await this.db.Value;

            var exampleData = new Dictionary<string, object>();

            // TODO fix this by using static reflection when we have time
            foreach (var propertyInfo in example.GetType().GetProperties())
            {
                if (!propertyInfo.PropertyType.IsGenericType)
                {
                    exampleData.Add(propertyInfo.Name, propertyInfo.GetValue(example, new object[] {}));
                }
            }

            var data = await db.Query<T>(collectionName, exampleData);

            LogReadMany("Loaded all documents", data, null);

            return data;
        }

        public async Task<IEnumerable<T>> LoadAll()
        {
            var db = await this.db.Value;

            var data = (await db.ReadAll<T>(collectionName)).ToList();

            LogReadMany("Loaded all documents", data, null);

            return data;
        }

        public async Task Upsert(string id, DocumentBase<T> item)
        {
            var db = await this.db.Value;

            await db.UpsertDocument(collectionName, id, item);

            LogSave("Saved {@vm} for {documentKey}", item.VM, id);
        }

        public Task SaveAsync(DocumentBase<T> item, Func<T, string> keyFunc)
        {
            return Upsert(keyFunc(item.VM), item);
        }
    }
}
