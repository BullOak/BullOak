namespace BullOak.Common.WebApi.Test.Unit
{
    using System;
    using Xunit;
    using System.Net.Http;
    using BullOak.Common.WebApi;
    using FluentAssertions;

    public class HttpRequestMessageCorrelationExtensionsTest
    {
        [Fact]
        public void GenerateClientCorrelationId_WhenCorrelationIdPresentInPropertyBag_ThenThisCorrelationIdIsIgnored()
        {
            // Arrange
            var request = new HttpRequestMessage();
            var propCorrelationId = Guid.NewGuid();
            request.Properties.Add(HttpRequestMessageCorrelationExtensions.CorrelationIdPropertyName, propCorrelationId);

            // Act
            var correlationId = request.GenerateClientCorrelationId();

            // Assert
            correlationId.Should().NotBe(propCorrelationId);
            correlationId.Should().NotBe(Guid.Empty);
        }

        [Fact]
        public void GenerateClientCorrelationId_WhenCorrelationIdPresentInPropertyBag_ThenThisCorrelationIdIsRemovedFromPropertyBag()
        {
            // Arrange
            var request = new HttpRequestMessage();
            var propCorrelationId = Guid.NewGuid();
            request.Properties.Add(HttpRequestMessageCorrelationExtensions.CorrelationIdPropertyName, propCorrelationId);

            // Act
            var correlationId = request.GenerateClientCorrelationId();

            // Assert
            request.Properties.Keys.Contains(HttpRequestMessageCorrelationExtensions.CorrelationIdPropertyName)
                .Should()
                .BeTrue();
            request.Properties[HttpRequestMessageCorrelationExtensions.CorrelationIdPropertyName].Should()
                .NotBe(propCorrelationId);
        }

        [Fact]
        public void GenerateClientCorrelationId_WhenNewCorrelationIdIsGenerated_ThenThisCorrelationIdPresentInPropertyBag()
        {
            // Arrange
            var request = new HttpRequestMessage();

            // Act
            var correlationId = request.GenerateClientCorrelationId();

            // Assert
            request.Properties.Keys.Contains(HttpRequestMessageCorrelationExtensions.CorrelationIdPropertyName)
                .Should()
                .BeTrue();
            request.Properties[HttpRequestMessageCorrelationExtensions.CorrelationIdPropertyName].Should()
                .Be(correlationId);
        }

        [Fact]
        public void GetClientCorrelationId_WhenCorrelationIdExistsInPropertyBag_ThenThisCorrelatuionIdIsReturned()
        {
            // Arrange
            var request = new HttpRequestMessage();
            var propCorrelationId = Guid.NewGuid();
            request.Properties.Add(HttpRequestMessageCorrelationExtensions.CorrelationIdPropertyName, propCorrelationId);

            // Act
            var correlationId = request.GetClientCorrelationId();

            // Assert
            correlationId.Should().Be(propCorrelationId);
        }

        [Fact]
        public void GetClientCorrelationId_WhenCorrelationIdExistsInHttpHeader_ThenThisCorrelatuionIdIsReturned()
        {
            // Arrange
            var request = new HttpRequestMessage();
            var httpCorrelationId = Guid.NewGuid();
            request.Headers.Add(HttpRequestMessageCorrelationExtensions.CorrelationIdHttpHeaderName, httpCorrelationId.ToString("D"));

            // Act
            var correlationId = request.GetClientCorrelationId();

            // Assert
            correlationId.Should().Be(httpCorrelationId);
        }

        [Fact]
        public void GetClientCorrelationId_WhenWhenCorrelationIdExistsInHttpHeaderAndCorrelationIdExistsInPropertyBag_ThenPropertyBagCorrelatuionIdIsReturned()
        {
            // Arrange
            var request = new HttpRequestMessage();
            var httpCorrelationId = Guid.NewGuid();
            request.Headers.Add(HttpRequestMessageCorrelationExtensions.CorrelationIdHttpHeaderName, httpCorrelationId.ToString("D"));
            var propCorrelationId = Guid.NewGuid();
            request.Properties.Add(HttpRequestMessageCorrelationExtensions.CorrelationIdPropertyName, propCorrelationId);

            // Act
            var correlationId = request.GetClientCorrelationId();

            // Assert
            correlationId.Should().Be(propCorrelationId);
        }

        [Fact]
        public void GetClientCorrelationId_WhenNoHeaderOrPropertyBagCorrelationIdProvided_ThenNoCorrelationIdIsReturned()
        {
            // Arrange
            var request = new HttpRequestMessage();

            // Act
            var correlationId = request.GetClientCorrelationId();

            // Assert
            correlationId.Should().Be(Guid.Empty);
        }
    }
}
