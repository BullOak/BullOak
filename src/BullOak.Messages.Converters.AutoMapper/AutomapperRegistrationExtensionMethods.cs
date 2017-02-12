//This is purposely on a parent namespace
// ReSharper disable once CheckNamespace
namespace BullOak.Messages
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BullOak.Messages.Converters;
    using BullOak.Messages.Converters.AutoMapper.Exceptions;
    using BullOak.Messages.Converters.AutoMapper.PreferenceAttributes;
    using AutoMapper;

    public static class AutomapperRegistrationExtensionMethods
    {
        private static readonly Type profileExpressionType = typeof(IProfileExpression);

        public static void ValidateConvertersAndRegisterMaps(this IMapperConfigurationExpression config,
            params IEventConverter[] converters)
        {
            ValidateConvertersAndRegisterMaps(config, (IEnumerable<IEventConverter>)converters);
        }

        public static void ValidateConvertersAndRegisterMaps(this IMapperConfigurationExpression config, IEnumerable<IEventConverter> converters)
        {
            if (converters == null) throw new ArgumentNullException(nameof(converters));

            foreach (var converter in converters)
            {
                var attributes = converter.GetType()
                    .CustomAttributes;

                if (attributes.Count(x =>
                            x.AttributeType == typeof(AutomaticallyCreateDefaultMappingsFromConverterGenericTypesAttribute)
                            || x.AttributeType == typeof(ThrowExceptionIfAutomapperRegistrationDoesNotExistAttribute)
                            || x.AttributeType == typeof(DoNotUseAutomapperAttribute))
                                > 1)
                {
                    throw new MultipleAutomapperRegistrationPreferencesDetectedException(converter.GetType());
                }

                if (attributes.Any(x => x.AttributeType == typeof(AutomaticallyCreateDefaultMappingsFromConverterGenericTypesAttribute)))
                {
                    config.CreateMap(converter.SourceType, converter.DestinationType);
                }
                else if (attributes.Any(x => x.AttributeType == typeof(DoNotUseAutomapperAttribute)))
                    continue;
                else if (
                    attributes.Any(
                        x => x.AttributeType == typeof(ThrowExceptionIfAutomapperRegistrationDoesNotExistAttribute)))
                {
                    var registerAutomapperMethod = converter
                        .GetType()
                        .GetMethods()
                        .SingleOrDefault(x =>
                            x.IsPublic && x.IsStatic && x.GetParameters().Length == 1 &&
                            x.GetParameters()[0].ParameterType == profileExpressionType);

                    if (registerAutomapperMethod != null)
                    {
                        registerAutomapperMethod.Invoke(null, new[] { config });
                    }
                    else
                    {
                        throw new AutomapperRegistrationRequiredException(converter.SourceType,
                            converter.DestinationType, converter.GetType());
                    }
                }
                else
                {
                    throw new AutomapperRegistrationPreferenceMissingException(converter.GetType());
                }
            }
        }
    }
}
