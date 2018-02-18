namespace BullOak.Repositories.Middleware
{
    using System;

    public interface IInterceptEvents
    {
        void BeforePublish(object @event, Type typeOfEvent, object state, Type typeOfState);
        void AfterPublish(object @event, Type typeOfEvent, object state, Type typeOfState);
        void BeforeSave(object @event, Type typeOfEvent, object state, Type typeOfState);
        void AfterSave(object @event, Type typeOfEvent, object state, Type typeOfState);
    }
}