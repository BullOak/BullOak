namespace BullOak.Repositories.Upconverting
{
    using System;
    using System.Text;

    [Serializable]
    public class PreflightUpconverterConflictException : Exception
    {
        public Type SourceEventType { get; }
        public Type DuplicateUpconverterType { get; }

        public PreflightUpconverterConflictException(Type typeOfSourceEvent, object duplicateUpconverter)
            :base(CreateMessage(typeOfSourceEvent, duplicateUpconverter))
        {
            SourceEventType = typeOfSourceEvent;
            DuplicateUpconverterType = duplicateUpconverter.GetType();
        }

        private static string CreateMessage(Type typeOfSourceEvent, object duplicateUpconverter) => new StringBuilder()
            .Append("Detected duplicate upconverter.")
            .AppendFormat(" The upconverter with type {0} has the same source event type as an existing upconverter.", duplicateUpconverter.GetType())
            .AppendFormat(" Source event type is {0}.", typeOfSourceEvent)
            .AppendFormat(" In case you need to upconvert one event to multiple please combine the two upconverters into one by implementing {0}.", typeof(IUpconvertEvent<,>).FullName)
            .Append(" If upconverter is the same type as the existing one please note that upconverters get registered FOR EACH per IUpconvertEvent interface implementation, once per source event")
            .ToString();
    }
}
