namespace BullOak.Messages.Converters
{
    using System;

    internal struct ConverterTuple
    {
        public Type SourceType { get; set; }
        public Type DestinationType { get; set; }
        public int ConverterCost { get; set; }
        public Func<IParcelVisionEvent, IParcelVisionEvent> ConvertFunction { get; set; }

        public ConverterTuple(Type sourceType, Type destinationType, int converterCost,
            Func<IParcelVisionEvent, IParcelVisionEvent> convertFunction)
        {
            SourceType = sourceType;
            DestinationType = destinationType;
            ConverterCost = converterCost;
            ConvertFunction = convertFunction;
        }
    }
}