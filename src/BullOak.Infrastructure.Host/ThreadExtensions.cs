namespace BullOak.Infrastructure.Host
{
    using System;
    using System.Diagnostics;
    using System.Threading;

    internal static class ThreadExtensions
    {
        private static readonly TimeSpan ThreadJoinTimeout = TimeSpan.FromSeconds(5);

        public static void SafeStop(this Thread thread)
        {
            thread.SafeStop(ThreadJoinTimeout);
        }

        public static void SafeStop(this Thread thread, TimeSpan timeout)
        {
            if (thread == null) return;

            try
            {
                if (!thread.Join(timeout))
                {
                    Trace.TraceError("Thread failed to exit gracefully");
                    thread.Abort();
                }
            }
            catch (Exception ex)
            {
                var error = ex.ToString();
                Trace.TraceError($"Failed to stop thread: {error}");
            }
        }
    }
}
