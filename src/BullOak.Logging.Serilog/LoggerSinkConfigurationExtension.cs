namespace Serilog.Sinks.Etw
{
    using System;
    using Serilog;
    using Serilog.Configuration;
    using Serilog.Events;
    using Serilog.Formatting;
    using Serilog.Formatting.Display;

    public static class LoggerSinkConfigurationExtension
    {
        private const string DefaultOutputTemplate = "[{Service}] [{SourceContext}] {Message} [CorrelationId:{CorrelationId}]"; 

        public static LoggerConfiguration Etw(
            this LoggerSinkConfiguration sinkConfiguration,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            string outputTemplate = DefaultOutputTemplate,
            IFormatProvider formatProvider = null)
        {
            if (sinkConfiguration == null)
            {
                throw new ArgumentNullException(nameof(sinkConfiguration));
            }

            if (outputTemplate == null)
            {
                throw new ArgumentNullException(nameof(outputTemplate));
            }

            var formatter = new MessageTemplateTextFormatter(outputTemplate, formatProvider);

            return Etw(
                sinkConfiguration,
                formatter,
                restrictedToMinimumLevel);
        }

        public static LoggerConfiguration Etw(
            this LoggerSinkConfiguration sinkConfiguration,
            ITextFormatter formatter,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum)
        {
            if (sinkConfiguration == null)
            {
                throw new ArgumentNullException(nameof(sinkConfiguration));
            }

            var sink = new EtwSink(formatter);

            return sinkConfiguration.Sink(sink, restrictedToMinimumLevel);
        }
    }
}
