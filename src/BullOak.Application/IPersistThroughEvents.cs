namespace BullOak.Application
{
    using Messages;

    internal interface IPersistThroughEvents
    {
        void ReconstituteFrom<TEvent>(TEvent eventEnvelopes)
            where TEvent : IParcelVisionEvent;
    }
}