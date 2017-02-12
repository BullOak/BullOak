namespace BullOak.Infrastructure.Host.Test.Unit
{
    using System.IO;
    using System.Linq;
    using Xunit;
    using BullOak.Infrastructure.Host;

    public class AppDomainIsolatedHostTests
    {
        private static readonly string location;

        static AppDomainIsolatedHostTests()
        {
            location = Directory.GetCurrentDirectory();
        }

        private bool IsIsolatedAppDomain(string appDomainName, string expectedName)
        {
            if (string.IsNullOrEmpty(appDomainName)) return false;
            return appDomainName.StartsWith($"Isolated:{expectedName}");
        }

        [Fact]
        public void AppDomainIsolatedHost_Create_IsolationAppDomainLoaded()
        {
            // Arrange
            const string appDomainName = @"testHost";
            Assert.False(AppDomainHelper.GetAppDomainNames().Any(d => IsIsolatedAppDomain(d, appDomainName)));

            // Act
            using (var isolatedHost = new AppDomainIsolatedHost<TestHost>(location, string.Empty, appDomainName))
            {

                // Assert
                Assert.NotNull(isolatedHost);
                Assert.True(AppDomainHelper.GetAppDomainNames().Any(d => IsIsolatedAppDomain(d, appDomainName)));
            }
        }

        [Fact]
        public void AppDomainIsolatedHost_Dispose_IsolationAppDomainUnloaded()
        {
            // Arrange
            const string appDomainName = @"testHost";

            // Act
            using (var isolatedHost = new AppDomainIsolatedHost<TestHost>(location, string.Empty, appDomainName))
            {
            }

            // Assert
            Assert.False(AppDomainHelper.GetAppDomainNames().Any(d => IsIsolatedAppDomain(d, appDomainName)));
        }

        [Fact]
        public void AppDomainIsolatedHost_StartHost_IsolationAppDomainLoaded()
        {
            // Arrange
            const string appDomainName = @"testHost";

            using (var isolatedHost = new AppDomainIsolatedHost<TestHost>(location, string.Empty, appDomainName))
            {
                // Act
                using (var handle = isolatedHost.Start())
                {
                    // Assert
                    Assert.True(AppDomainHelper.GetAppDomainNames().Any(d => IsIsolatedAppDomain(d, appDomainName)));
                }
            }
        }

        [Fact]
        public void AppDomainIsolatedHost_DisposeHost_IsolationAppDomainLoaded()
        {
            // Arrange
            const string appDomainName = @"testHost";

            using (var isolatedHost = new AppDomainIsolatedHost<TestHost>(location, string.Empty, appDomainName))
            {
                // Act
                using (var handle = isolatedHost.Start())
                {
                }

                // Assert
                Assert.True(AppDomainHelper.GetAppDomainNames().Any(d => IsIsolatedAppDomain(d, appDomainName)));
            }
        }
    }
}
