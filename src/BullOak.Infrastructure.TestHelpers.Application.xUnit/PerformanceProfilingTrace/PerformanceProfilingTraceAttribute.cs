namespace BullOak.Infrastructure.TestHelpers.Application.xUnit
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.Reflection;
    using System.Threading;
    using Xunit.Sdk;

    public class PerformanceProfilingTraceAttribute : BeforeAfterTestAttribute
    {
        private static readonly bool performanceProfilingTraceEnabled;
        private readonly Dictionary<string, PerformanceProfilingTracer> tracersDictionary = new Dictionary<string, PerformanceProfilingTracer>();
        private readonly object tracersLock = new object();


        private string GetTracerKeyForMethod(MethodInfo methodInfo)
        {
            return methodInfo.Name + "#" + Thread.CurrentThread.ExecutionContext.GetHashCode().ToString();
        }

        private PerformanceProfilingTracer GetTracerForMethod(MethodInfo methodInfo)
        {
            var key = GetTracerKeyForMethod(methodInfo);
            PerformanceProfilingTracer tracer;

            if (tracersDictionary.ContainsKey(key))
            {
                tracer = tracersDictionary[key];
            }
            else
            {
                tracer = new PerformanceProfilingTracer(key);
                tracersDictionary.Add(key, tracer);
            }

            return tracer;
        }

        private void RemoveTracerForMethod(MethodInfo methodInfo)
        {
            var key = GetTracerKeyForMethod(methodInfo);
            if (tracersDictionary.ContainsKey(key))
            {
                tracersDictionary.Remove(key);
            }
        }

        public override void Before(MethodInfo methodUnderTest)
        {
            if (performanceProfilingTraceEnabled)
            {
                lock (tracersLock)
                {
                    GetTracerForMethod(methodUnderTest).Start();
                }
            }
        }

        public override void After(MethodInfo methodUnderTest)
        {
            if (performanceProfilingTraceEnabled)
            {
                PerformanceSummary summary;
                lock (tracersLock)
                {
                    var tracer = GetTracerForMethod(methodUnderTest);
                    summary = tracer.Stop();

                    RemoveTracerForMethod(methodUnderTest);
                }

                Trace.WriteLine(summary.ToString());
            }
        }

        static PerformanceProfilingTraceAttribute()
        {
            var setting = ConfigurationManager.AppSettings["bulloak-xunit.performanceProfilingTraceEnabled"];
            if (!string.IsNullOrWhiteSpace(setting))
            {
                bool.TryParse(setting, out performanceProfilingTraceEnabled);
            }
            else
            {
                performanceProfilingTraceEnabled = false;
            }
        }
    }
}
