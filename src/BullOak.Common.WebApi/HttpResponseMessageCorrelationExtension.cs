namespace BullOak.Common.WebApi
{
    using System;
    using System.Linq;
    using System.Net.Http;

    public static class HttpResponseMessageCorrelationExtension
    {
        /// <summary>
        /// The Http header for client correlation id - "x-correlation-id"
        /// </summary>
        private const string CorrelationIdHttpHeaderName = "x-correlation-id";

        public static Guid GetCorrelationIdFromResponseHeader(this HttpResponseMessage response)
        {
            //read the correlation from response and return
            if (response != null)
            {
                if (response.Headers.Contains(CorrelationIdHttpHeaderName))
                {
                    var headers = response.Headers.GetValues(CorrelationIdHttpHeaderName);
                    var headerCorrelationId = headers.FirstOrDefault(s => s != null);

                    Guid requestCorrelationId;
                    Guid.TryParse(headerCorrelationId, out requestCorrelationId);
                    return requestCorrelationId;
                }               
            }
            return Guid.Empty;
        }
    }
}