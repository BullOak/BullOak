namespace Serilog.Sinks.Etw
{
    public class LogContext
    {
        public string CorrelationId { get; set; }
        public string ServiceName { get; set; }
        public string EnvironmentUserName { get; set; }
        public string EnvironmentId { get; set; }
        public string SourceContext { get; set; }
        public string RenderedMessage { get; set; }
    }
}
