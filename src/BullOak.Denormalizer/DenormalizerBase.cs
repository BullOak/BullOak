namespace BullOak.Denormalizer
{
    using System;
    using System.Threading.Tasks;
    public abstract class DenormalizerBase<T> where T : class, new()
    {
        private readonly IDenormalizerRepository<T> repository;
        private readonly string repositoryKey;

        protected DenormalizerBase(IDenormalizerRepository<T> repository, string viewModelKey = null)
        {
            if (repository == null) throw new ArgumentNullException(nameof(repository));

            this.repository = repository;
            repositoryKey = viewModelKey ?? typeof(T).FullName;
        }

        protected virtual async Task Process<TEvent>(TEvent @event, string id)
        {
            var viewModel = await repository.GetById(id) ?? new DocumentBase<T>();

            Map(@event, viewModel.VM);

            await repository.Upsert(id, viewModel);
        }

        protected abstract T Map<TEvent>(TEvent @event, T viewModel);
    }
}
