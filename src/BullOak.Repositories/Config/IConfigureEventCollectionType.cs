namespace BullOak.Repositories
{
    using StateTypeToCollectionTypeSelector = System.Func<System.Type, System.Func<System.Collections.Generic.ICollection<object>>>;

    public interface IConfigureEventCollectionType
    {
        IConfigureStateFactory WithEventCollectionSelector(StateTypeToCollectionTypeSelector selectorOfCollectionTypeFromStateType);
    }
}