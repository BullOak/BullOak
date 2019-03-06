namespace BullOak.Repositories
{
    /// <summary>
    /// The delivery guarantee that is the target of this process. There are multiple places where
    /// this needs to be considered and it cannot be only handled in infrastructure. From the perspective
    /// of publishing chosing AtLeastOnce means that events are published before being saved (so
    /// if there is a failiure during the saving process, multiples of each event will be published
    /// on the bus. Conversely, the AtMostOnce option means that only after successful save process
    /// will the events be published to the bus, so if there is a failure after saving, one or all
    /// of the events may not be published.
    ///
    /// This choice may be the singularly more important choice in the design of your ecosystem
    /// and it must be viewed as a tradeoff between complexity and correctness: chosing AtLeastOnce
    /// will allow you a more correct system at some eventual time, but at a significantly more
    /// complex one at the same time, with the increased possibility of very subtle bugs. On the
    /// flipside chosing AtMostOnce may mean that there is a really low percentage of possibility
    /// that an event may be missed and not published.
    /// </summary>
    public enum DeliveryTargetGuarntee { AtMostOnce, AtLeastOnce }
}