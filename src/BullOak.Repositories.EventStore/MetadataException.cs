using System;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;

namespace BullOak.Repositories.EventStore
{
    [Serializable]
    internal class MetadataException : Exception
    {
        private JObject asJson;

        public MetadataException(JObject asJson, string message)
            : base(message)
        {
            this.asJson = asJson;
        }
    }
}