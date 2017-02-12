namespace BullOak.Infrastructure.Host
{
    using System;
    using System.Diagnostics;
    using System.Security.Policy;
    using System.Threading;

    public class AppDomainIsolatedHost<T> : IDisposable
        where T : HostBase
    {
        private AppDomain domain;
        private bool disposed;

        private class IsolatedHostHandle : IDisposable
        {
            private readonly AppDomainIsolatedHost<T> isolation;
            private readonly IDisposable handle;

            public IsolatedHostHandle(AppDomainIsolatedHost<T> isolation, IDisposable handle)
            {
                if (isolation == null) throw new ArgumentNullException(nameof(isolation));
                if (handle == null) throw new ArgumentNullException(nameof(handle));

                this.isolation = isolation;
                this.handle = handle;
            }

            public void Dispose()
            {
                try
                {
                    handle?.Dispose();
                }
                catch (Exception ex)
                {
                    var error = ex.ToString();
                    Trace.TraceError($"Failed to stop AppDomainIsolatedHost: {error}");
                }

                try
                {
                    isolation?.Dispose();
                }
                catch (Exception ex)
                {
                    var error = ex.ToString();
                    Trace.TraceError($"Failed to unload AppDomainIsolatedHost: {error}");
                }
            }
        }

        public static IDisposable StartHost(string hostName, string applicationBase, string applicationConfig)
        {
            try
            {
                var hostIsolation = new AppDomainIsolatedHost<T>(applicationBase, applicationConfig, hostName);
                var hostHandle = hostIsolation.Start();

                return new IsolatedHostHandle(hostIsolation, hostHandle);
            }
            catch (Exception ex)
            {
                var error = ex.ToString();
                Trace.TraceError($"Failed to start AppDomainIsolatedHost: {error}");
                throw;
            }
        }

        public AppDomainIsolatedHost(string applicationBase, string applicationConfig, string name)
        {
            var setup = new AppDomainSetup
            {
                ApplicationBase = applicationBase,
                PrivateBinPath = applicationBase,
                ConfigurationFile = applicationConfig
            };

            // Set up the Evidence
            Evidence baseEvidence = AppDomain.CurrentDomain.Evidence;
            Evidence evidence = new Evidence(baseEvidence);

            try
            {
                domain = AppDomain.CreateDomain(
                    $"Isolated:{name}" + Guid.NewGuid(),
                    evidence,
                    setup);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Unhandled exception while creating AppDomain: " + ex.ToString());
                throw;
            }
        }

        public IDisposable Start()
        {
            Type type = typeof(T);
            var instance = (T)domain.CreateInstanceAndUnwrap(type.Assembly.FullName, type.FullName);
            return instance.Start();
        }

        ~AppDomainIsolatedHost()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                // in finalizer
                return;
            }

            if (disposed)
            {
                return;
            }

            const int MaxAppDomainUnloadRetries = 5;

            var i = 0;
            while ((domain != null) && (i < MaxAppDomainUnloadRetries))
            {
                try
                {
                    AppDomain.Unload(domain);
                    domain = null;
                }
                catch (AppDomainUnloadedException)
                {
                    // already unloaded
                    domain = null;
                }
                catch (Exception ex)
                {
                    var error = ex.ToString();
                    Trace.TraceWarning($"Unhandled exception while unloading AppDomain {domain.FriendlyName}: {error}");
                    Thread.Sleep(((i + 1) * 100));
                }
                finally
                {
                    i++;
                }
            }

            if (domain != null)
            {
                Trace.TraceError($"Failed to unload AppDomain {domain.FriendlyName}");
            }

            disposed = true;
        }
    }
}
