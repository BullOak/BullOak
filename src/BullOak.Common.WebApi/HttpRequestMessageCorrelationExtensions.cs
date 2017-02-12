namespace BullOak.Common.WebApi
{
    using System;
    using System.Net.Http;
    using System.Linq;

    public static class HttpRequestMessageCorrelationExtensions
    {
        /// <summary>
        /// The Http header for client correlation id - "x-correlation-id"
        /// </summary>
        public const string CorrelationIdHttpHeaderName = "x-correlation-id";

        /// <summary>
        /// The internal property name for client correlation id
        /// </summary>
        internal const string CorrelationIdPropertyName = "ParcelVision.CorrelationId";

        public static Guid GenerateClientCorrelationId(this HttpRequestMessage request)
        {
            if (request.Properties.ContainsKey(CorrelationIdPropertyName))
            {
                request.Properties.Remove(CorrelationIdPropertyName);
            }

            var correlationId = Guid.NewGuid();

            request.Properties.Add(CorrelationIdPropertyName, correlationId);

            return correlationId;
        }

        public static Guid GetClientCorrelationId(this HttpRequestMessage request)
        {
            // if correlation id is already present return that
            object correlationId;
            if (request.Properties.TryGetValue(CorrelationIdPropertyName, out correlationId))
            {
                return (Guid)correlationId; ;
            }

            // if correlation id is not already present fetch it from the request headers (if possible)
            if (request.Headers.Contains(CorrelationIdHttpHeaderName))
            {
                var headers = request.Headers.GetValues(CorrelationIdHttpHeaderName);
                var headerCorrelationId = headers.FirstOrDefault(s => s != null);

                Guid requestCorrelationId;
                if (Guid.TryParse(headerCorrelationId, out requestCorrelationId))
                {
                    request.Properties.Add(CorrelationIdPropertyName, requestCorrelationId);
                    return requestCorrelationId;
                }
            }

            return Guid.Empty; // no correlation id initialized
        }
    }
}