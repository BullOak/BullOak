namespace BullOak.Common.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class ConfigurationException : Exception
    {
        public ConfigurationException(string message, Exception innerException = null) : base(message, innerException)
        {

        }

        protected ConfigurationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            
        }
    }
}