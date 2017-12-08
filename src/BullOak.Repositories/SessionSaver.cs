namespace BullOak.Repositories
{
    using System;
    using System.Threading.Tasks;
    using BullOak.Repositories.Session;

    internal class SessionSaver : ISaveSessionChanges
    {
        private static Task Done = Task.FromResult(0);

        private readonly Action save;
        private readonly Func<Task> saveAsync;
        private readonly Action publish;
        private readonly Func<Task> publishAsync;

        public SessionSaver(Action save, Func<Task> saveAsync, Action publish, Func<Task> publishAsync)
        {
            if (!(save == null ^ saveAsync == null)) throw new ArgumentNullException($"Exactly one of {nameof(save)} or {nameof(saveAsync)} has to be non-null", (Exception)null);
            if (!(publish == null ^ publishAsync == null)) throw new ArgumentNullException($"Exactly one of {nameof(publish)} or {nameof(publishAsync)} has to be non-null", (Exception)null);

            this.save = save;
            this.saveAsync = saveAsync ?? (() =>
            {
                save();
                return Done;
            });
            this.publish = publish;
            this.publishAsync = publishAsync ?? (() =>
            {
                publish();
                return Done;
            });
        }

        public Task SaveChanges(DeliveryTargetGuarntee targetGuarantee = DeliveryTargetGuarntee.AtLeastOnce)
            => (save != null && publish != null) ? SaveChangesSync(targetGuarantee, save, publish) : SaveChangesAsync(targetGuarantee);

        private async Task SaveChangesAsync(DeliveryTargetGuarntee targetGuarantee)
        {
            if (targetGuarantee == DeliveryTargetGuarntee.AtLeastOnce) await publishAsync();

            await saveAsync();

            if (targetGuarantee == DeliveryTargetGuarntee.AtMostOnce) await publishAsync();
        }

        private static Task SaveChangesSync(DeliveryTargetGuarntee targetGuarantee, Action save, Action publish)
        {
            if (targetGuarantee == DeliveryTargetGuarntee.AtLeastOnce) publish();

            save();

            if (targetGuarantee == DeliveryTargetGuarntee.AtMostOnce) publish();

            return Done;
        }
    }
}
