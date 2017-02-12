namespace Serilog.Sinks.Etw
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Serilog.Events;
    using Serilog.Formatting;
    using Serilog.Sinks.PeriodicBatching;

    public sealed class EtwSink : PeriodicBatchingSink 
    {
        private readonly ITextFormatter formatter;

        public EtwSink(
            ITextFormatter formatter)
            : base(1000, TimeSpan.FromSeconds(0.5))
        {
            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            this.formatter = formatter;
        }

        private string ExtractContextProperty(LogEvent logEvent, string property)
        {
            var value = string.Empty;
            LogEventPropertyValue propertyValue;

            if (logEvent.Properties.TryGetValue(property, out propertyValue))
            {
                value = propertyValue?.ToString().Replace('"', ' ').Trim();
            }

            return value;
        }

        protected override async Task EmitBatchAsync(IEnumerable<LogEvent> events)
        { 
            foreach (var logEvent in events)
            {
                using (var stringWriter = new StringWriter())
                {
                    formatter.Format(logEvent, stringWriter);

                    var logContext = new LogContext()
                    {
                        CorrelationId = ExtractContextProperty(logEvent, "CorrelationId"),
                        ServiceName = ExtractContextProperty(logEvent, "Service"),
                        EnvironmentUserName = ExtractContextProperty(logEvent, "EnvironmentUserName"),
                        EnvironmentId = ExtractContextProperty(logEvent, "EnvironmentId"),
                        SourceContext = ExtractContextProperty(logEvent, "SourceContext"),
                        RenderedMessage = logEvent.RenderMessage().Replace('"', ' ').Trim()
                    };

                    var message = stringWriter.ToString();

                    switch (logEvent.Level)
                    {
                        case LogEventLevel.Debug:
                        case LogEventLevel.Verbose:

                            // ETW has no notion of DEBUG so we will map it to Verbose
                            ServiceEventSource.Log.WriteVerboseEvent(logContext, message);
                            break;

                        case LogEventLevel.Information:

                            ServiceEventSource.Log.WriteInformationEvent(logContext, message);
                            break;

                        case LogEventLevel.Warning:

                            ServiceEventSource.Log.WriteWarningEvent(logContext, message);
                            break;

                        case LogEventLevel.Error:

                            ServiceEventSource.Log.WriteErrorEvent(logContext, message,
                                logEvent?.Exception.ToString());
                            break;

                        case LogEventLevel.Fatal:

                            ServiceEventSource.Log.WriteCriticalEvent(logContext, message,
                                logEvent?.Exception.ToString());
                            break;
                    }
                }
            }
            
        }
    }
}
