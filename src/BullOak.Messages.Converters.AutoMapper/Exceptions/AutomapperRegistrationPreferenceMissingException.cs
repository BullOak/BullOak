namespace BullOak.Messages.Converters.AutoMapper.Exceptions
{
    using System;
    using PreferenceAttributes;

    public class AutomapperRegistrationPreferenceMissingException : Exception
    {
        public AutomapperRegistrationPreferenceMissingException(Type upconverterType)
            : base($@"{upconverterType.Name} does not declare how to handle Automapper map registrations. Please decorate the upconverter with one of 
                {nameof(DoNotUseAutomapperAttribute)}, 
                {nameof(AutomaticallyCreateDefaultMappingsFromConverterGenericTypesAttribute)} or 
                {nameof(ThrowExceptionIfAutomapperRegistrationDoesNotExistAttribute)}")
        { }

        public AutomapperRegistrationPreferenceMissingException(string message)
            : base(message)
        { }
    }
}