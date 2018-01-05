namespace BullOak.Repositories.Exceptions
{
    using System;
    using BullOak.Repositories.Session;

    public class EntityDoesNotExistException : Exception
    {
        /// <summary>Initializes a new instance of the <see cref="T:System.Exception" /> class.</summary>
        public EntityDoesNotExistException(string id)
            :base($"Entity with id {id} does not exist. Please use {nameof(IManagePersistenceOf<object, IManageAndSaveSession<object>,object>.BeginSessionFor)} with optional parameter `throwIfNotExists` set to false.")
        { }
    }
}