namespace BullOak.Repositories.StateEmit.Emitters
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Reflection.Emit;

    internal class OwnedStateClassEmitter : BaseClassEmitter
    {
        internal override bool CanEmitWith => true;

        public override Type EmitType(ModuleBuilder modelBuilder, Type typeToMake, string nameToUseForType = null)
            => base.EmitType(modelBuilder, typeToMake, "OwneddStateEmitter_" + (string.IsNullOrWhiteSpace(
                                                                                       nameToUseForType)
                                                                                       ? typeToMake.Name
                                                                                       : nameToUseForType));

        public sealed override void ClassSetup(TypeBuilder typeBuilder)
        { }
        public sealed override void EmitCtor(TypeBuilder typeBuilder)
        { }

        public sealed override FieldBuilder PropertySetup(TypeBuilder typeBuilder, PropertyInfo propInfo) =>
            typeBuilder.DefineField($"_{propInfo.Name}", propInfo.PropertyType,
                FieldAttributes.Private | FieldAttributes.HasDefault);

        public sealed override void EmitGetValueOpCodes(ILGenerator getMethodGenerator, FieldBuilder fieldToStoreValue)
        {
            getMethodGenerator.Emit(OpCodes.Ldarg_0);
            getMethodGenerator.Emit(OpCodes.Ldfld, fieldToStoreValue);
            getMethodGenerator.Emit(OpCodes.Ret);
        }

        public sealed override void EmitSetValueOpCodes(ILGenerator setMethodGenerator, FieldBuilder fieldToStoreValue)
        {
            setMethodGenerator.Emit(OpCodes.Ldarg_0);
            setMethodGenerator.Emit(OpCodes.Ldarg_1);
            setMethodGenerator.Emit(OpCodes.Stfld, fieldToStoreValue);
            setMethodGenerator.Emit(OpCodes.Ret);
        }

        /// <inheritdoc />
        /// <inheritdoc />
        public override void EmitWithMethod(TypeBuilder typeBuilder, Type typeToMake, Type valueTypeType, List<Tuple<FieldBuilder, Type, PropertyInfo>> propertiesAndFields)
        {
            var methodFactory = typeBuilder.DefineMethod("With", MethodAttributes.Final
                    | MethodAttributes.HideBySig
                    | MethodAttributes.Public
                    | MethodAttributes.NewSlot
                    | MethodAttributes.Virtual,
                CallingConventions.HasThis);

            string[] typeParameterNames = { "TProp" };
            GenericTypeParameterBuilder[] typeParameters =
                methodFactory.DefineGenericParameters(typeParameterNames);

            var openFuncType = typeof(Func<,>);
            var constructedFuncType = openFuncType.MakeGenericType(typeToMake, typeParameters[0]);
            var openExpressionType = typeof(Expression<>);
            var getBodyMethod= openExpressionType.GetProperty("Body").GetMethod;
            var constructedExpressionType = openExpressionType.MakeGenericType(constructedFuncType);


            methodFactory.SetParameters(constructedExpressionType, typeParameters[0]);
            methodFactory.SetReturnType(typeToMake);

            var ilgen = methodFactory.GetILGenerator();

            Label notSupportedExceptionLabel = ilgen.DefineLabel();
            Label breakLabel = ilgen.DefineLabel();
            Label defaultLabel = ilgen.DefineLabel();

            var propertyLabels = new Dictionary<string, Label>();
            var propertyInfo = new Dictionary<string, Tuple<FieldBuilder, Type, PropertyInfo>>();

            foreach (var prop in propertiesAndFields)
            {
                propertyLabels[prop.Item3.Name] = ilgen.DefineLabel();
                propertyInfo[prop.Item3.Name] = prop;
            }

            var copy = ilgen.DeclareLocal(typeBuilder);
            var expression = ilgen.DeclareLocal(typeof(MemberExpression));
            var propertyName = ilgen.DeclareLocal(typeof(string));


            // Shallow copy from this and store in local 0
            ilgen.Emit(OpCodes.Ldarg_0);
            ilgen.EmitCall(OpCodes.Call, typeof(object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic), new Type[0]);
            ilgen.Emit(OpCodes.Castclass, typeBuilder);
            ilgen.Emit(OpCodes.Stloc_0);

            // if(expressionArgument.Body is MemberExpression expression)
            ilgen.Emit(OpCodes.Ldarg_1);
            ilgen.Emit(OpCodes.Callvirt, getBodyMethod);
            ilgen.Emit(OpCodes.Isinst, typeof(MemberExpression));
            ilgen.Emit(OpCodes.Stloc_1);
            ilgen.Emit(OpCodes.Ldloc_1);
            ilgen.Emit(OpCodes.Brfalse_S, notSupportedExceptionLabel);

            //switch(expression.Member.Name)
            ilgen.Emit(OpCodes.Ldloc_1);
            ilgen.Emit(OpCodes.Callvirt, typeof(MemberExpression).GetProperty("Member").GetMethod);
            ilgen.Emit(OpCodes.Callvirt, typeof(MemberInfo).GetProperty("Name").GetMethod);
            ilgen.Emit(OpCodes.Stloc_2);

            //Branching on input
            ilgen.Emit(OpCodes.Ldloc_2);
            ilgen.Emit(OpCodes.Brfalse_S, defaultLabel);
            var stringEqualsOp = typeof(string).GetMethod("op_Equality", BindingFlags.Public | BindingFlags.Static);

            foreach (var key in propertyLabels.Keys)
            {
                ilgen.Emit(OpCodes.Ldloc_2);
                ilgen.Emit(OpCodes.Ldstr, key);
                ilgen.Emit(OpCodes.Call, stringEqualsOp);
                ilgen.Emit(OpCodes.Brtrue_S, propertyLabels[key]);
            }

            //Emit property setting
            foreach (var propLabels in propertyLabels)
            {
                ilgen.MarkLabel(propLabels.Value);
                ilgen.Emit(OpCodes.Ldloc_0);
                ilgen.Emit(OpCodes.Ldarg_2);
                ilgen.Emit(OpCodes.Stfld, propertyInfo[propLabels.Key].Item1);

                //Break
                ilgen.Emit(OpCodes.Br_S, breakLabel);
            }

            // Emit default case
            ilgen.MarkLabel(defaultLabel);
            ilgen.ThrowException(typeof(NotSupportedException));

            // Return copy
            ilgen.MarkLabel(breakLabel);
            ilgen.Emit(OpCodes.Ldloc_0);
            ilgen.Emit(OpCodes.Ret);

            // NOTE -> end of if (not explicit, based on jumps)

            //Emit throw exception as end of method
            ilgen.MarkLabel(notSupportedExceptionLabel);
            ilgen.ThrowException(typeof(NotSupportedException));
        }
    }
}
