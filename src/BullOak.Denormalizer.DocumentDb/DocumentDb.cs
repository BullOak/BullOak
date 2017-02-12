namespace BullOak.Denormalizer.DocumentDb
{
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using BullOak.EventStream;

    //Note: this is meant to be used a singleton through the DI layer
    public class DocumentDb : IDocumentStore
    {
        private readonly string dbId;
        private readonly DocumentClient client;

        private class DocumentWrapper : IDocumentBase
        {
            [JsonProperty(propertyName: "id")]
            public string Id { get; set; }
            [JsonProperty(propertyName: "_etag")]
            public string ETag { get; set; }

            public static DocumentWrapper<T> FromBase<T>(DocumentBase<T> @base, string id)
            {
                return new DocumentWrapper<T>
                {
                    Id = id,
                    ETag = @base.ETag,
                    VM = @base.VM
                };
            }
        }

        private class DocumentWrapper<T> : DocumentWrapper
        {
            public T VM { get; set; }

            public DocumentBase<T> ToBase()
            {
                return new DocumentBase<T>()
                {
                    ETag = ETag,
                    VM = VM,
                };
            }
        }

        public DocumentDb(string location, string key, string dbId)
        {
            if (string.IsNullOrWhiteSpace(location)) throw new ArgumentNullException(nameof(location));
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
            if (string.IsNullOrWhiteSpace(dbId)) throw new ArgumentNullException(nameof(dbId));

            this.dbId = dbId;

            client = new DocumentClient(new Uri(location), key);
        }

        private string GetValueString(object value)
        {
            if (value is string || value is Guid)
            {
                return $"'{value}'";
            }

            return value.ToString();
        }

        public async Task UpsertDocument<T>(string collectionId, string documentId, DocumentBase<T> item)
        {
            if (string.IsNullOrWhiteSpace(collectionId)) throw new ArgumentNullException(nameof(collectionId));
            if (string.IsNullOrWhiteSpace(documentId)) throw new ArgumentNullException(nameof(documentId));
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (item.VM == null) throw new ArgumentNullException(nameof(item.VM));
            if (string.IsNullOrWhiteSpace(item.ETag)) throw new ArgumentNullException(nameof(item.ETag));

            var accessOption = new AccessCondition { Condition = item.ETag, Type = AccessConditionType.IfMatch };

            var collectionUri = UriFactory.CreateDocumentCollectionUri(dbId, collectionId);
            try
            {
                //Here we need to await so as to convert the exception to the correct, domain-specific one.
                await client.UpsertDocumentAsync(collectionUri, DocumentWrapper.FromBase(item, documentId),
                    new RequestOptions { AccessCondition = accessOption }, true);
            }
            catch (DocumentClientException ex) when (ex.StatusCode == HttpStatusCode.PreconditionFailed || ex.StatusCode == HttpStatusCode.Conflict)
            {
                throw new ConcurrencyException(ex.Message, JsonConvert.SerializeObject(item));
            }
        }

        public bool DocumentExists(string collectionId, string documentId)
        {
            // TODO (Savvas): Add framework logging
            if (string.IsNullOrWhiteSpace(collectionId)) throw new ArgumentNullException(nameof(collectionId));
            if (string.IsNullOrWhiteSpace(documentId)) throw new ArgumentNullException(nameof(documentId));

            try
            {
                return client.CreateDocumentQuery(UriFactory.CreateDocumentCollectionUri(dbId, collectionId))
                    .Where(doc => doc.Id == documentId)
                    .Select(doc => doc.Id)
                    .AsEnumerable()
                    .Any();
            }
            catch (Exception e)
            {
                if (e?.InnerException?.Message?.Contains("Resource Not Found") == true)
                {
                    // TODO (Savvas): Add framework logging
                    return false;
                }

                throw;
            }
        }

        public async Task<DocumentBase<T>> ReadDocument<T>(string collectionId, string documentId)
        {
            if (string.IsNullOrWhiteSpace(collectionId)) throw new ArgumentNullException(nameof(collectionId));
            if (string.IsNullOrWhiteSpace(documentId)) throw new ArgumentNullException(nameof(documentId));

            // TODO (Savvas): Add framework logging

            var documentUri = UriFactory.CreateDocumentUri(dbId, collectionId, documentId);

            try
            {
                var responce = await client.ReadDocumentAsync(documentUri);
                var document = responce.Resource;

                DocumentWrapper<T> wrapper = (dynamic)document;
                return wrapper.ToBase();
            }
            catch (DocumentClientException e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                return default(DocumentBase<T>);
            }
        }

        public async Task<IEnumerable<T>> ReadAll<T>(string collectionId)
        {
            if (string.IsNullOrWhiteSpace(collectionId)) throw new ArgumentNullException(nameof(collectionId));

            // TODO (Savvas): Add framework logging

            try
            {
                var collectionUri = UriFactory.CreateDocumentCollectionUri(dbId, collectionId);

                //.Cast<DocumentWrapper<T>>() does not work. In general casting a Document using 'as'
                // return null. Please read: https://msdn.microsoft.com/en-us/library/System.Dynamic.IDynamicMetaObjectProvider.aspx
                return (await client.ReadDocumentFeedAsync(collectionUri))
                    .Select(x => ((DocumentWrapper<T>) x).VM)
                    .ToList();
            }
            catch (DocumentClientException e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                return Enumerable.Empty<T>();
            }
        }

        public Task<IEnumerable<DocumentBase<T>>> ReadPaged<T>(string collectionId, int skip, int take)
        {
            // TODO (Savvas): Add framework logging

            throw new NotImplementedException();
        }

        // TODO change the signature to something more appropriate (better type safety)
        public Task<IEnumerable<T>> Query<T>(string collectionId, Dictionary<string, object> filterProperties)
        {
            if (string.IsNullOrWhiteSpace(collectionId)) throw new ArgumentNullException(nameof(collectionId));
            if (filterProperties == null) throw new ArgumentNullException(nameof(filterProperties));

            // TODO (Savvas): Add framework logging

            var queryOptions = new FeedOptions { MaxItemCount = -1 };
            var collectionUri = UriFactory.CreateDocumentCollectionUri(dbId, collectionId);

            var query = new StringBuilder("SELECT * FROM c");

            // TODO don't put '' around anything other than strings

            var isStart = true;
            foreach (var kvp in filterProperties)
            {
                query.Append(isStart ? " WHERE " : " AND ");
                query.Append($"c.VM.{kvp.Key} = {GetValueString(kvp.Value)}");               
                isStart = false;
            }

            // TODO (Savvas): Add framework logging

            IEnumerable<T> items = client.CreateDocumentQuery<DocumentBase<T>>(
                    collectionUri,
                    query.ToString(),
                    queryOptions)
                .AsEnumerable()
                .Select(x => x.VM)
                .ToList();

            return Task.FromResult(items);
        }

        public async Task DeleteDocument(string collectionId, string documentId)
        {
            if (string.IsNullOrWhiteSpace(collectionId)) throw new ArgumentNullException(nameof(collectionId));
            if (string.IsNullOrWhiteSpace(documentId)) throw new ArgumentNullException(nameof(documentId));

            // TODO (Savvas): Add framework logging

            try
            {
                var documentUri = UriFactory.CreateDocumentUri(dbId, collectionId, documentId);

                await client.DeleteDocumentAsync(documentUri);
            }
            catch (DocumentClientException dce) when (dce.StatusCode == HttpStatusCode.NotFound)
            { }
        }

        public async Task<bool> CollectionExists(string collectionId)
        {
            if (string.IsNullOrWhiteSpace(collectionId)) throw new ArgumentNullException(nameof(collectionId));

            // TODO (Savvas): Add framework logging

            var collectionUri = UriFactory.CreateDocumentCollectionUri(dbId, collectionId);

            try
            {
                await client.ReadDocumentCollectionAsync(collectionUri);
            }
            catch (DocumentClientException dce)
                when (dce.StatusCode != null && dce.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // TODO (Savvas): Add framework logging
                return false;
            }

            // TODO (Savvas): Add framework logging
            return true;
        }

        //TODO (Savvas): We need a better strategy for this, but should be good enough.!
        public async Task CreateCollection(string collectionId)
        {
            // TODO (Savvas): Add framework logging

            var exists = await CollectionExists(collectionId);

            if (!exists)
            {
                var docCollection = new DocumentCollection
                {
                    Id = collectionId,
                    IndexingPolicy = new IndexingPolicy
                    {
                        Automatic = true,
                        IndexingMode = IndexingMode.Consistent,
                    },
                    ResourceId = collectionId,
                };

                var dbUri = UriFactory.CreateDatabaseUri(dbId);
                await client.CreateDocumentCollectionAsync(dbUri, docCollection);
            }
        }

        public async Task DeleteCollection(string collectionId)
        {
            if (string.IsNullOrWhiteSpace(collectionId)) throw new ArgumentNullException(nameof(collectionId));

            // TODO (Savvas): Add framework logging

            try
            {
                var collectionUrl = UriFactory.CreateDocumentCollectionUri(dbId, collectionId);
                await client.DeleteDocumentCollectionAsync(collectionUrl);
            }
            catch (DocumentClientException dce)
                when (dce.StatusCode == System.Net.HttpStatusCode.NotFound)
            { }
        }

        public async Task ClearCollection(string collectionId)
        {
            if (string.IsNullOrWhiteSpace(collectionId)) throw new ArgumentNullException(nameof(collectionId));

            // TODO (Savvas): Add framework logging

            try
            {
                var collectionUrl = UriFactory.CreateDocumentCollectionUri(dbId, collectionId);
                var docs = client.CreateDocumentQuery(collectionUrl);

                var tasks = new List<Task>();
                foreach (var doc in docs)
                {
                    tasks.Add(client.DeleteDocumentAsync(doc.SelfLink));
                }

                await Task.WhenAll(tasks.ToArray());
            }
            catch (DocumentClientException dce)
                when (dce.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
            }
            catch (AggregateException aex)
                when (aex.InnerExceptions.All(x=> (x as DocumentClientException)?.StatusCode == HttpStatusCode.NotFound))
            { }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                client.Dispose();
            }
        }
    }
}
