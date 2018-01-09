namespace BullOak.Repositories.Exceptions
{
    using System;
    using BullOak.Repositories.Session;

    public class StreamNotFoundException : Exception
    {
        /// <summary>Initializes a new instance of the <see cref="T:System.Exception" /> class.</summary>
        public StreamNotFoundException(string id)
            : base($"Stream for id: {id} does not exist. Please use with optional parameter `throwIfNotExists` set to false.")
        { }
    }
}