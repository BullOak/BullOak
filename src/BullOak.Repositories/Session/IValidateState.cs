namespace BullOak.Repositories.Session
{
    public interface IValidateState<in TState>
    {
        ValidationResults Validate(TState state);
    }
}
