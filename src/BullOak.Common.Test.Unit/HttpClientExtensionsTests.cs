using BullOak.Common.WebApi.Extensions;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
namespace BullOak.Common.Test.Unit
{
    using System.Collections.Generic;
    using System.IO;
    using FluentAssertions;
    using Newtonsoft.Json;
    using SuperSimple.MiniWebServer;

    public class HttpClientExtensionsTests
    {
        public class Arrangements
        {
            private readonly string Scheme = System.Uri.UriSchemeHttp;
            private readonly string Server = "localhost";
            private readonly int Port = 19216;

            public class RequestInfo
            {
                public string Method { get; }
                public string Resource { get; }
                public string Payload { get; }
                public string ContentType { get; }
                public DateTime TimeStamp { get; } = DateTime.UtcNow;

                public RequestInfo(string method, string resource, string payload, string contentType)
                {
                    Method = method;
                    Resource = resource;
                    Payload = payload;
                    ContentType = contentType;
                }
            }

            public List<RequestInfo> Requests { get; } = new List<RequestInfo>();
            public string LastResourceAccessed { get; set; }
            public IDisposable StartServer()
            {
                return SuperSimple.MiniWebServer.Configuration.Start()
                    .SetHostAddress(Scheme, Server, Port)
                    .WithMiddleware()
                    .AddCustomController(Capture)
                    .Build()();
            }

            public HttpClient GetClient() => new HttpClient()
            {
                BaseAddress = new Uri($"{Scheme}://{Server}:{Port}")
            };

            private async Task<MiddlewareInvocationEnum> Capture(Environment env)
            {
                var method = env.RequestMethod;
                var resource = env.RequestPath;
                var contentType = env.RequestHeaders["Content-Type"][0];
                string payload;
                using (var reader = new StreamReader(env.RequestBody))
                {
                    payload = await reader.ReadToEndAsync();
                }

                Requests.Add(new RequestInfo(method, resource, payload, contentType));

                return MiddlewareInvocationEnum.StopChain;
            }
        }

        [Fact]
        public async Task PatchAsJsonAsync_WithNullUri_ThrowsArgumentException()
        {
            //Arrange
            var client = new Arrangements().GetClient();

            //Act
            var exception = await Record.ExceptionAsync(() => client.PatchAsJsonAsync(null, new object()));

            //Assert
            exception.Should().NotBeNull();
            exception.Should().BeOfType<ArgumentNullException>();
        }

        [Fact]
        public async Task PatchAsJsonAsync_WithNullContent_ThrowsArgumentException()
        {
            //Arrange
            var client = new Arrangements().GetClient();

            //Act
            var exception = await Record.ExceptionAsync(() => client.PatchAsJsonAsync("asd", (object)null));

            //Assert
            exception.Should().NotBeNull();
            exception.Should().BeOfType<ArgumentNullException>();
        }

        [Fact]
        public async Task PatchAsJsonAsync_WithValidParams_ShouldMakeExactlyOneRequestToResource()
        {
            //Arrange
            var arrangements = new Arrangements();
            var payload = new { Value = 5, Name = "MyName" };

            //Act
            using (arrangements.StartServer())
            using(var client = arrangements.GetClient())
            {
                await client.PatchAsJsonAsync("capture", payload);
            }

            //Arrange
            arrangements.Requests.Should().NotBeEmpty();
            arrangements.Requests.Count.Should().Be(1);
            var request = arrangements.Requests[0];
            request.Resource.Should().EndWith("capture");
        }

        [Fact]
        public async Task PatchAsJsonAsync_WithValidParams_ShouldMakeAPatchRequestToResource()
        {
            //Arrange
            var arrangements = new Arrangements();
            var payload = new { Value = 5, Name = "MyName" };

            //Act
            using (arrangements.StartServer())
            using (var client = arrangements.GetClient())
            {
                await client.PatchAsJsonAsync("capture", payload);
            }

            //Arrange
            var request = arrangements.Requests[0];
            request.Method.Should().BeEquivalentTo("patch");
        }

        [Fact]
        public async Task PatchAsJsonAsync_WithValidParams_RequestShouldBeJson()
        {
            //Arrange
            var arrangements = new Arrangements();
            var payload = new { Value = 5, Name = "MyName" };

            //Act
            using (arrangements.StartServer())
            using (var client = arrangements.GetClient())
            {
                await client.PatchAsJsonAsync("capture", payload);
            }

            //Arrange
            var request = arrangements.Requests[0];
            request.ContentType.Should().BeEquivalentTo("application/json; charset=utf-8");
        }

        [Fact]
        public async Task PatchAsJsonAsync_WithValidParams_RequestShouldIncludePayload()
        {
            //Arrange
            var arrangements = new Arrangements();
            var payload = new { Value = 5, Name = "MyName" };

            //Act
            using (arrangements.StartServer())
            using (var client = arrangements.GetClient())
            {
                await client.PatchAsJsonAsync("capture", payload);
            }

            //Arrange
            var request = arrangements.Requests[0];
            var receivedPayload = JsonConvert.DeserializeAnonymousType(request.Payload, payload);
            receivedPayload.Value.Should().Be(payload.Value);
            receivedPayload.Name.Should().Be(payload.Name);
        }
    }
}
