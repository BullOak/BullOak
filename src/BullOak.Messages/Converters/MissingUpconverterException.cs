namespace BullOak.Messages.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class MissingUpconverterException : Exception
    {
        public MissingUpconverterException(IEnumerable<string> eventsMissingAnUpconverter)
            : base($"Detected events without an upconverter to them: {eventsMissingAnUpconverter.Aggregate("", (aggregated, typeName) => $"{aggregated} {typeName} -")}")
        { }
    }
}
