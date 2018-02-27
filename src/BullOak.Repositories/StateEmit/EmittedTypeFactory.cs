namespace BullOak.Repositories.StateEmit
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Reflection.Emit;
    using BullOak.Repositories.StateEmit.Emitters;

    internal class EmittedTypeFactory : BaseTypeFactory
    {
        private ConcurrentDictionary<Type, Func<object>> TypeFactories = new ConcurrentDictionary<Type, Func<object>>();

        public override void WarmupWith(IEnumerable<Type> typesToCreateFactoriesFor)
        {
            base.WarmupWith(typesToCreateFactoriesFor);
            lock (TypeFactories)
            {
                foreach(var type in typesToCreateFactoriesFor)
                {
                    if (!TypeFactories.ContainsKey(type))
                    {
                        TypeFactories[type] = CreateTypeFactoryMethod(type);
                    }
                }
            }
        }

        public override object GetState(Type type)
        {
            if (TypeFactories.TryGetValue(type, out var factory)) return factory();

            lock (TypeFactories)
            {
                if (!TypeFactories.ContainsKey(type))
                {
                    TypeFactories[type] = CreateTypeFactoryMethod(type);
                }
            }

            return TypeFactories[type]();
        }

        internal static Func<object> CreateTypeFactoryMethod_Old(Type type)
        {
            var typeToCreate = type.IsInterface ? StateTypeEmitter.EmitType(type, new OwnedStateClassEmitter()) : type;

            var ctor = typeToCreate.GetConstructor(Type.EmptyTypes);

            if (ctor == null || !ctor.IsPublic)
                throw new ArgumentException("Requested type has to have a default ctor", nameof(type));

            return Expression.Lambda<Func<object>>(Expression.New(ctor)).Compile();
        }

        private static Func<object> CreateTypeFactoryMethod(Type type)
        {
            var typeToCreate = type.IsInterface ? StateTypeEmitter.EmitType(type, new OwnedStateClassEmitter()) : type;

            if (!typeToCreate.IsValueType)
            {
                var ctor = typeToCreate.GetConstructor(Type.EmptyTypes);

                if (ctor == null || !ctor.IsPublic)
                    throw new ArgumentException("Requested type has to have a default ctor", nameof(type));

                DynamicMethod ctorCall = new DynamicMethod("GetInstanceOf",
                    MethodAttributes.Public | MethodAttributes.Static,
                    CallingConventions.Standard,
                    typeToCreate,
                    null,
                    typeof(EmittedTypeFactory),
                    true);

                var il = ctorCall.GetILGenerator(8);
                il.Emit(OpCodes.Newobj, ctor);
                il.Emit(OpCodes.Ret);

                return (Func<object>) ctorCall.CreateDelegate(typeof(Func<object>));
            }
            else
            {
                var value = Activator.CreateInstance(typeToCreate);

                //Value types will be copied
                return () => value;
            }
        }

    }
}
