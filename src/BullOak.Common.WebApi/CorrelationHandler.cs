namespace BullOak.Common.WebApi
{
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    public class CorrelationHandler : DelegatingHandler
    {         
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            AddCorrelationIdTo(request.GetClientCorrelationId(), response);

            return response;
        }

        private void AddCorrelationIdTo(Guid correlationId, HttpResponseMessage response)
        {
            if (response != null)
            {
                // make sure the response has the correlation id in the header
                if (!response.Headers.Contains(HttpRequestMessageCorrelationExtensions.CorrelationIdHttpHeaderName))
                {
                    response.Headers.Add(
                        HttpRequestMessageCorrelationExtensions.CorrelationIdHttpHeaderName,
                        correlationId.ToString("D"));
                }
            }
        }
    }
}
