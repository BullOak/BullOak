namespace BullOak.Repositories.Session
{
    using System;
    using System.Threading.Tasks;

    internal class FunctionSessionSaver : ISaveSessionChanges
    {
        private readonly Func<DeliveryTargetGuarntee, Task> saveFunc;

        private FunctionSessionSaver(Func<DeliveryTargetGuarntee, Task> saveFunc) => this.saveFunc = saveFunc;
        private FunctionSessionSaver(Func<Task> saveFunc) => this.saveFunc = _ => saveFunc();
        public Task SaveChanges(DeliveryTargetGuarntee targetGuarantee) => saveFunc(targetGuarantee);

        public static explicit operator FunctionSessionSaver(Func<Task> saveFunc) => new FunctionSessionSaver(saveFunc);
        public static explicit operator FunctionSessionSaver(Func<DeliveryTargetGuarntee, Task> saveFunc) => new FunctionSessionSaver(saveFunc);
    }
}