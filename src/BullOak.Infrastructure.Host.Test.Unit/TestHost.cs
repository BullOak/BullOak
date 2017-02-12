namespace BullOak.Infrastructure.Host.Test.Unit
{
    using System;
    using System.IO;

    public class TestHost : HostBase
    {
        public override IDisposable Start()
        {
            return new TestHostStarter();
        }

        [Serializable]
        private class TestHostStarter : IDisposable
        {
            private MemoryStream _resource;

            public TestHostStarter()
            {
                _resource = new MemoryStream();
            }

            public void Dispose()
            {
                if (_resource != null)
                {
                    _resource.Dispose();
                    _resource = null;
                }
            }
        }
    }
}
