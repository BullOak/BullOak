namespace BullOak.Common.WebApi.Test.Unit
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Web.Http.Controllers;
    using System.Web.Http.Hosting;
    using Xunit;
    using System.Threading.Tasks;
    using FluentAssertions;
    using BullOak.Common.WebApi;
    
    public class CorrelationHandlerTest
    {
        public class InnerHandlerMock : DelegatingHandler
        {
            private HttpResponseMessage responseToReturn;

            public void AssertResponse(HttpResponseMessage response)
            {
                this.responseToReturn = response;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                if (this.responseToReturn == null)
                {
                    return base.SendAsync(request, cancellationToken);
                }

                return Task.Factory.StartNew(() => this.responseToReturn, cancellationToken);
            }
        }

        private static HttpRequestMessage CreateRequest(HttpMethod method, string url)
        {
            var request = new HttpRequestMessage(method, url);
            request.Content = new ByteArrayContent(new byte[] { });
            request.Properties[HttpPropertyKeys.RequestContextKey] = new HttpRequestContext() { Configuration = new System.Web.Http.HttpConfiguration() };

            return request;
        }

        public CorrelationHandler SetupCorrelationHandler()
        {
            var handler = new CorrelationHandler();

            var innerHandler = new InnerHandlerMock();
            innerHandler.AssertResponse(new HttpResponseMessage(HttpStatusCode.OK));
            handler.InnerHandler = innerHandler;

            return handler;
        }

        [Fact]
        public async Task AddCorrelationIdTo_NoCorrelationIdInRequestHeader_ExpectNewCorrelationIdInResponseHeader()
        {
            // Arrange
            var handler = this.SetupCorrelationHandler();
            var invoker = new HttpMessageInvoker(handler);
            var request = CreateRequest(HttpMethod.Get, "http://localhost:9090/api/values");

            // Act
            var response = await invoker.SendAsync(request, new CancellationToken(false));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Headers.Contains(HttpRequestMessageCorrelationExtensions.CorrelationIdHttpHeaderName).Should().BeTrue();
            response.Headers.GetValues(HttpRequestMessageCorrelationExtensions.CorrelationIdHttpHeaderName)
                .ToList()
                .Count.Should()
                .Be(1);
            request.GetClientCorrelationId()
                .ToString()
                .Should()
                .Be(response.Headers.GetValues(HttpRequestMessageCorrelationExtensions.CorrelationIdHttpHeaderName).ToArray()[0]);
        }

        [Fact]
        public async Task AddCorrelationIdTo_CorrelationIdInRequestHeader_ExpectCorrelationIdInRequest()
        {
            // Arrange
            var handler = this.SetupCorrelationHandler();
            var invoker = new HttpMessageInvoker(handler);
            var request = CreateRequest(HttpMethod.Get, "http://localhost:9090/api/values");
            var corrId = Guid.NewGuid();
            request.Headers.Add(HttpRequestMessageCorrelationExtensions.CorrelationIdHttpHeaderName, corrId.ToString());

            // Act
            var response = await invoker.SendAsync(request, new CancellationToken(false));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            request.GetClientCorrelationId()
                .Should()
                .Be(corrId);
        }

        [Fact]
        public async Task AddCorrelationIdTo_CorrelationIdInRequestHeader_ExpectCorrelationIdInResponseHeader()
        {
            // Arrange
            var handler = this.SetupCorrelationHandler();
            var invoker = new HttpMessageInvoker(handler);
            var request = CreateRequest(HttpMethod.Get, "http://localhost:9090/api/values");
            var corrId = Guid.NewGuid().ToString();
            request.Headers.Add(HttpRequestMessageCorrelationExtensions.CorrelationIdHttpHeaderName, corrId);

            // Act
            var response = await invoker.SendAsync(request, new CancellationToken(false));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Headers.Contains(HttpRequestMessageCorrelationExtensions.CorrelationIdHttpHeaderName).Should().BeTrue();
            response.Headers.GetValues(HttpRequestMessageCorrelationExtensions.CorrelationIdHttpHeaderName)
                .ToList()
                .Count.Should()
                .Be(1);
            response.Headers.GetValues(HttpRequestMessageCorrelationExtensions.CorrelationIdHttpHeaderName).ToArray()[0].Should()
                .Be(corrId);
        }
    }
}