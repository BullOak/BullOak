namespace BullOak.Repositories
{
    public static class ThreadSafetySelectorExtensions
    {
        /// <summary>
        /// This method will cause BullOak.Repositories to automatically use thread safe operations for all operations of any state type. This is generally safer than the alternative
        /// but this comes at a significant overhead. MOREOVER: when using messaging you should generally avoid using threading and rely on messaging to achieve utilization of hardware
        /// resources, which mean that there is no need for synchronization.
        /// </summary>
        /// <param name="self"></param>
        /// <returns>An object that will be responsible for configuring the event publisher.</returns>
        public static IConfigureEventPublisher AlwaysUseThreadSafe(this IConfigureThreadSafety self)
            => self.WithThreadSafetySelector(_ => true);

        /// <summary>
        /// This method will cause BullOak.Repositories to never use thread safe operations for any operation of any state type. This is generally less safe than the alternative but
        /// this comes at a significant performance gain. MOREOVER: when using messaging you should generally avoid using threading and rely on messaging to achieve utilization of hardware
        /// resources, which mean that there is no need for synchronization.
        /// </summary>
        /// <param name="self"></param>
        /// <returns>An object that will be responsible for configuring the event publisher.</returns>
        public static IConfigureEventPublisher NeverUseThreadSafe(this IConfigureThreadSafety self)
            => self.WithThreadSafetySelector(_ => false);
    }
}