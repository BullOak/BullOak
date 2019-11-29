namespace BullOak.Repositories.Test.Unit.UpconverterContainer
{
    using System.Collections.Generic;
    using BullOak.Repositories.Upconverting;

    public class EventA
    { }

    public class EventB
    { }

    public class EventC
    { }

    internal class InternalEvent
    { }

    public class UpconverterAToB : IUpconvertEvent<EventA, EventB>
    {
        public EventB Upconvert(EventA source) => throw new System.NotImplementedException();
    }

    public class UpconverterBToMany : IUpconvertEvent<EventB>
    {
        public IEnumerable<object> Upconvert(EventB source) => throw new System.NotImplementedException();
    }

    internal class InternalUpconvertPublicEvent : IUpconvertEvent<EventC>
    {
        public IEnumerable<object> Upconvert(EventC source) => throw new System.NotImplementedException();
    }

    internal class InternalUpconvertInternalEvent : IUpconvertEvent<InternalEvent>
    {
        public IEnumerable<object> Upconvert(InternalEvent source) => throw new System.NotImplementedException();
    }
}
