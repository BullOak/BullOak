namespace BullOak.Application.Test.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Caching;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using Willow.Application.Ids;
    using Willow.Application.UserTrigger;
    using Xunit;

    public class CachedAggregateRepositoryTest
    {
        private class Arrangements
        {
            public Mock<IAggregateRepository<UserTriggerAggregate, UserTriggerId>> InnerRepositoryMock { get; set; }
            public CachedAggregateRepository<UserTriggerAggregate, UserTriggerId> Sut { get; set; }
            public string Id { get; set; }
        }

        private Arrangements GetArrangements()
        {
            var arrangements = new Arrangements()
            {
                Id = Guid.NewGuid().ToString(),
                InnerRepositoryMock = new Mock<IAggregateRepository<UserTriggerAggregate, UserTriggerId>>(), 
            };

            arrangements.Sut =
                new CachedAggregateRepository<UserTriggerAggregate, UserTriggerId>(
                    arrangements.InnerRepositoryMock.Object);

            return arrangements;
        }

        public CachedAggregateRepositoryTest()
        {
            var cacheKeys = MemoryCache.Default.Select(kvp => kvp.Key).ToList();
            foreach (var cacheKey in cacheKeys)
                MemoryCache.Default.Remove(cacheKey);
        }

        [Fact]
        public void CachedAggregateRepository_NullInnerRepository_ThrowsException()
        {
            // Arrange
            // Act
            var exception = Record.Exception(() => new CachedAggregateRepository<UserTriggerAggregate, UserTriggerId>(null));

            // Assert
            exception.Should().NotBeNull();
            exception.Should().BeOfType<ArgumentNullException>();
        }

        [Fact]
        public async Task CachedAggregateRepository_IdInCache_CacheReturnsTrueWithoutAccessingRepository()
        {
            // Arrange
            var arrangement = GetArrangements();
            MemoryCache.Default.Add(arrangement.Id, bool.TrueString, null);

            // Act
            var exists = await arrangement.Sut.Exists(arrangement.Id);

            // Assert
            exists.Should().BeTrue();
            arrangement.InnerRepositoryMock.Verify(x => x.Exists(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task CachedAggregateRepository_IdNotInCacheButInRepository_ReturnsTrueAndCacheIsAdded()
        {
            // Arrange
            var arrangement = GetArrangements();
            arrangement.InnerRepositoryMock.Setup(x => x.Exists(It.Is<string>(s => s == arrangement.Id)))
                .Returns(Task.FromResult(true));
          

            // Act
            var exists = await arrangement.Sut.Exists(arrangement.Id);

            // Assert
            exists.Should().BeTrue();
            MemoryCache.Default.Contains(arrangement.Id).Should().BeTrue();
        }

        [Fact]
        public async Task CachedAggregateRepository_IdNotInCacheAndNotInRepository_ReturnsFalseAndCacheIsNotAdded()
        {
            // Arrange
            var arrangement = GetArrangements();
            arrangement.InnerRepositoryMock.Setup(x => x.Exists(It.Is<string>(s => s == arrangement.Id)))
                .Returns(Task.FromResult(false));


            // Act
            var exists = await arrangement.Sut.Exists(arrangement.Id);

            // Assert
            exists.Should().BeFalse();
            MemoryCache.Default.Contains(arrangement.Id).Should().BeFalse();
        }

    }
}
