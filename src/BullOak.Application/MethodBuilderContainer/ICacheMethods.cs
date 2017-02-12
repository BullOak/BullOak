namespace BullOak.Application.MethodBuilderContainer
{
    internal interface ICacheMethods
    {
        object Invoke(object targetInstance, params object[] parameters);
    }
}