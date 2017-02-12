namespace Serilog.Sinks.Etw
{
    using System.Diagnostics;
    using System.Diagnostics.Tracing;
    using System.Threading.Tasks;

    [EventSource(Name = "BullOak-Serilog-0001")]
    public class ServiceEventSource : EventSource
    {
        public class Keywords
        {
            public const EventKeywords Diagnostic = (EventKeywords)1;
        }

        public class Tasks
        {

        }

        static ServiceEventSource()
        {
            // A workaround for the problem where ETW activities do not get tracked until Tasks infrastructure is initialized.
            // This problem will be fixed in .NET Framework 4.6.2.
            Task.Run(() => { });
        }

        private static ServiceEventSource log = new ServiceEventSource();
        private ServiceEventSource() : base() { }
        public static ServiceEventSource Log { get { return log; } }

        [NonEvent]
        public void WriteVerboseEvent(LogContext logContext, string message)
        {
            if (IsEnabled())
            {
                Verbose(logContext.CorrelationId, logContext.ServiceName, logContext.EnvironmentUserName, logContext.EnvironmentId, logContext.SourceContext, logContext.RenderedMessage, message);
            }
        }

        private const int VerboseEventId = 35;
        [Event(VerboseEventId, Message = "{6}", Level = EventLevel.Verbose)]
        internal void Verbose(
            string correlationId,
            string serviceName,
            string environmentUserName,
            string environmentId,
            string sourceContext,
            string renderedMessage,
            string message)
        {
            if (IsEnabled())
            {
                this.WriteEvent(VerboseEventId, correlationId, serviceName, environmentUserName, environmentId, sourceContext, renderedMessage, message);
            }
        }

        [NonEvent]
        public void WriteInformationEvent(LogContext logContext, string message)
        {
            if (IsEnabled())
            {
                Information(logContext.CorrelationId, logContext.ServiceName, logContext.EnvironmentUserName, logContext.EnvironmentId, logContext.SourceContext, logContext.RenderedMessage, message);
            }
        }

        private const int InformationEventId = 40;
        [Event(InformationEventId, Message = "{6}", Level = EventLevel.Informational)]
        internal void Information(
            string correlationId,
            string serviceName,
            string environmentUserName,
            string environmentId,
            string sourceContext,
            string renderedMessage,
            string message)
        {
            if (IsEnabled())
            {
                this.WriteEvent(InformationEventId, correlationId, serviceName, environmentUserName, environmentId, sourceContext, renderedMessage, message);
            }
        }

        [NonEvent]
        public void WriteWarningEvent(LogContext logContext, string message)
        {
            if (IsEnabled())
            {
                Warning(logContext.CorrelationId, logContext.ServiceName, logContext.EnvironmentUserName, logContext.EnvironmentId, logContext.SourceContext, logContext.RenderedMessage, message);
            }
        }

        private const int WarningEventId = 45;
        [Event(WarningEventId, Message = "{6}", Level = EventLevel.Warning)]
        internal void Warning(
            string correlationId,
            string serviceName,
            string environmentUserName,
            string environmentId,
            string sourceContext,
            string renderedMessage,
            string message)
        {
            if (IsEnabled())
            {
                this.WriteEvent(WarningEventId, correlationId, serviceName, environmentUserName, environmentId, sourceContext, renderedMessage, message);
            }
        }

        [NonEvent]
        public void WriteErrorEvent(LogContext logContext, string message, string exception)
        {
            if (IsEnabled())
            {
                Error(logContext.CorrelationId, logContext.ServiceName, logContext.EnvironmentUserName, logContext.EnvironmentId, logContext.SourceContext, exception, logContext.RenderedMessage, message);
            }
        }

        private const int ErrorEventId = 50;
        [Event(ErrorEventId, Message = "{7}", Level = EventLevel.Error)]
        internal void Error(
            string correlationId,
            string serviceName,
            string environmentUserName,
            string environmentId,
            string sourceContext,
            string exception,
            string renderedMessage,
            string message)
        {
            if (IsEnabled())
            {
                this.WriteEvent(ErrorEventId, correlationId, serviceName, environmentUserName, environmentId, sourceContext, exception, renderedMessage, message);
            }
        }

        [NonEvent]
        public void WriteCriticalEvent(LogContext logContext, string message, string exception)
        {
            if (IsEnabled())
            {
                Critical(logContext.CorrelationId, logContext.ServiceName, logContext.EnvironmentUserName, logContext.EnvironmentId, logContext.SourceContext, exception, logContext.RenderedMessage, message);
            }
        }

        private const int CriticalEventId = 55;
        [Event(CriticalEventId, Message = "{7}", Level = EventLevel.Critical)]
        internal void Critical(
            string correlationId,
            string serviceName,
            string environmentUserName,
            string environmentId,
            string sourceContext,
            string exception,
            string renderedMessage,
            string message)
        {
            if (IsEnabled())
            {
                this.WriteEvent(CriticalEventId, correlationId, serviceName, environmentUserName, environmentId, sourceContext, exception, renderedMessage, message);
            }
        }
    }
}
