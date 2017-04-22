namespace BullOak.Common.WebApi
{
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;

    public class HttpResponseAcceptedResult : IHttpActionResult
    {
        private object responseBody;
        private HttpRequestMessage request;

        public HttpResponseAcceptedResult(HttpRequestMessage requestMessage, object body = null)
        {
            responseBody = body ?? new
            {
                acknowledged = true
            };

            request = requestMessage;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
            => Task.FromResult(request.CreateResponse(HttpStatusCode.Accepted, responseBody));
    }
}
