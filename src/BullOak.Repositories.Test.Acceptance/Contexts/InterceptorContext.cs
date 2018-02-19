namespace BullOak.Repositories.Test.Acceptance.Contexts
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using BullOak.Repositories.Middleware;

    public class InterceptorContext: IInterceptEvents
    {
        private ConcurrentQueue<string> methodsCalled = new ConcurrentQueue<string>();
        public IEnumerable<string> MethodsCalled => methodsCalled.ToArray();

        /// <inheritdoc />
        public void BeforePublish(object @event, Type typeOfEvent, object state, Type typeOfState)
            => methodsCalled.Enqueue(nameof(BeforePublish));

        /// <inheritdoc />
        public void AfterPublish(object @event, Type typeOfEvent, object state, Type typeOfState)
            => methodsCalled.Enqueue(nameof(AfterPublish));

        /// <inheritdoc />
        public void BeforeSave(object @event, Type typeOfEvent, object state, Type typeOfState)
            => methodsCalled.Enqueue(nameof(BeforeSave));

        /// <inheritdoc />
        public void AfterSave(object @event, Type typeOfEvent, object state, Type typeOfState)
            => methodsCalled.Enqueue(nameof(AfterSave));
    }
}
