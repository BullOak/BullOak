namespace BullOak.Messages.Converters.AutoMapper.Exceptions
{
    using System;

    public class MultipleAutomapperRegistrationPreferencesDetectedException : Exception
    {
        public MultipleAutomapperRegistrationPreferencesDetectedException(Type upconverterType)
            : base($@"{upconverterType.Name} has more than one automapper registration preference attribute. Please decorate the upconverter with only one.")
        { }
    }
}
