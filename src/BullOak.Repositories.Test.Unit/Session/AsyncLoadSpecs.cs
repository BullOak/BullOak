using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace BullOak.Repositories.Test.Unit.Session
{
    public class AsyncLoadSpecs
    {
        [Fact]
        public async Task BeginSession_WithTwoEventsLoadedAsynchronously_ShouldSucceed()
        {
            //Arrange
            int id = Random.Shared.Next();
            int lastCount = Random.Shared.Next();
            var sut = new ConfigurationStub<TestState>()
                .WithDefaultSetup()
                .WithEventApplier(new CountChangedApplier())
                .AndLoadedAsynchronously()
                .GetNewSUT<int>()
                .WithEventInStream(new CountChangedClassEvent()
                {
                    NewCount = Random.Shared.Next()
                }, id)
                .WithEventInStream(new CountChangedClassEvent()
                {
                    NewCount = lastCount
                }, id);

            //Act
            var state = (await sut.BeginSessionFor(id, true)).GetCurrentState();

            //Assert
            state.Should().NotBeNull();
            state.Count.Should().Be(lastCount);
        }
    }
}
