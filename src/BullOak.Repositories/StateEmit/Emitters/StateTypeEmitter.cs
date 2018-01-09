namespace BullOak.Repositories.StateEmit.Emitters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Threading;

    internal class StateTypeEmitter
    {
        private static int emittedIndex = 0;

        public static Type EmitType(Type typeToMake, BaseClassEmitter emitterToUse, string nameForClass = null)
        {
            var modelBuilder = GetModelBuilder();

            if (!typeToMake.IsInterface)
                throw new ArgumentException($"Parameter must be type of an interface", nameof(typeToMake));
            if (typeToMake.GetMethods().Any(m => !m.IsHideBySig))
                throw new ArgumentException("Parameter must be of an interface type that does contain methods.",
                    nameof(typeToMake));

            return emitterToUse.EmitType(modelBuilder, typeToMake, nameForClass);
        }

        public static Dictionary<Type, Type> EmitTypes(BaseClassEmitter emitterToUse, params Type[] typesToMake)
        {
            var modelBuilder = GetModelBuilder();

            if (typesToMake.Any(x => !x.IsInterface))
                throw new ArgumentException($"Parameter must be type of an interface", nameof(typesToMake));
            if (typesToMake.Any(x => x.GetMethods().Any(m => !m.IsHideBySig)))
                throw new ArgumentException("Parameter must be of an interface type that does contain methods.",
                    nameof(typesToMake));

            return EmitTypes(emitterToUse, modelBuilder, typesToMake).ToDictionary(x => x.Item1, x => x.Item2);
        }

        private static IEnumerable<Tuple<Type, Type>> EmitTypes(BaseClassEmitter emitterToUse, ModuleBuilder modelBuilder, Type[] typesToMake)
            => typesToMake.Select(x => new Tuple<Type, Type>(x, emitterToUse.EmitType(modelBuilder, x)));

        private static ModuleBuilder GetModelBuilder()
        {
            var assemblyName = Assembly.GetExecutingAssembly().GetName();
            assemblyName.Name += ".Emitter.Group" + Interlocked.Increment(ref emittedIndex);
            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            return assemblyBuilder.DefineDynamicModule(assemblyName.Name);
        }
    }
}
