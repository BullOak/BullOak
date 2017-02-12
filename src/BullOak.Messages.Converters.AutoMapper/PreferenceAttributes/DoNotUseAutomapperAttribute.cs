namespace BullOak.Messages.Converters.AutoMapper.PreferenceAttributes
{
    using System;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class DoNotUseAutomapperAttribute : Attribute
    { }
}