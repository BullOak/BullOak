namespace BullOak.Application
{
    using System;
    using System.Threading.Tasks;

    public static class Retry
    {
        public static Func<double, double, int, double> ConstantDelayPolicy = (s, d, i) => s;
        public static Func<double, double, int, double> LinearIncreaseDelayPolicy = (s, d, i) => s + (i > 1 ? (i - 1) * d : 0);
        public static Func<double, double, int, double> ExponentialIncreaseDelayPolicy = (s, d, i) => s + (i > 1 ? Math.Pow(d, (i - 1)) : 0);
     
        public static async Task RetryOnException<T, Te>(
            Func<T, Task> call,
            T aggregate,
            int retryLimit, TimeSpan retryMinInterval, TimeSpan retryMaxInterval, TimeSpan retryDelta,
            Func<double, double, int, double> retryPolicy,
            Action<string, Te> retryErrorLogger = null)
            where Te : Exception
        {
            if (call == null) throw new ArgumentNullException(nameof(call));

            bool retrying = false;
            int retry = 0;
            do
            {
                try
                {
                    retrying = false;
                    await call(aggregate);
                }
                catch (Te ex)
                {
                    if (retry++ < retryLimit)
                    {
                        retrying = true;
                        var delay = TimeSpan.FromMilliseconds(retryPolicy(retryMinInterval.TotalMilliseconds, retryDelta.TotalMilliseconds, retry));
                        var retryInterval = delay > retryMaxInterval ? retryMaxInterval : delay;

                        retryErrorLogger?.Invoke($"Retrying exception: retry {retry}, delay {retryInterval}", ex);

                        await Task.Delay(retryInterval);
                    }
                    else
                    {
                        retryErrorLogger?.Invoke($"Operation failed after {retryLimit} retries", ex);
                        throw;
                    }
                }
            } while (retrying);
        }
    }
}
