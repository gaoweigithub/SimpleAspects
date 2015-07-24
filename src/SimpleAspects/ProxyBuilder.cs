using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;

namespace Simple
{
    internal static class ProxyBuilder<TInterfaceType>
    {
        private static readonly Type interfaceType = typeof(TInterfaceType);

        private static Type _proxyType;
        private static Func<TInterfaceType, TInterfaceType> _builder;
        private static Exception _buildException;

        private static bool IsReady
        {
            get
            {
                return _buildException != null || _proxyType != null;
            }
        }

        public static Type ProxyType
        {
            get
            {
                if (!IsReady)
                    CreateProxyBuilder();

                if (_buildException != null)
                    throw _buildException;

                return _proxyType;
            }
        }
        public static Func<TInterfaceType, TInterfaceType> Builder
        {
            get
            {
                if (!IsReady)
                    CreateProxyBuilder();

                if (_buildException != null)
                    throw _buildException;

                return _builder;
            }
        }

        public static void ClearCache()
        {
            _builder = null;
            _buildException = null;
            _proxyType = null;
        }


        private static void CreateProxyBuilder()
        {
            if (AspectFactory.GlobalAspects.Count == 0 && !interfaceType.GetMethods(BindingFlags.Public | BindingFlags.Instance).Any(i => i.GetCustomAttributes(typeof(AspectAttribute), true) != null))
                _builder = (i) => i;

            try
            {
                _proxyType = BuildType();
            }
            catch (Exception ex)
            {
                _buildException = ex;
                _builder = (o) => { throw ex; };
            }

            Delegate deleg = Delegate.CreateDelegate(typeof(Func<TInterfaceType, TInterfaceType>), ProxyType.GetMethod("CreateProxy", BindingFlags.Public | BindingFlags.Static));
            _builder = (Func<TInterfaceType, TInterfaceType>)deleg;
        }

        private static Type BuildType()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");

            if (!interfaceType.IsInterface)
                throw new NotSupportedException("An interface was expected, but a type was found: " + interfaceType.FullName);

            if (!interfaceType.IsPublic)
                throw new NotSupportedException("The proxy interface must be public.");

            var module = SimpleModuleBuilder.Instance;

            TypeBuilder typeBuilder = module.DefineType(interfaceType.Name.Substring(1) + "SimpleProxy", TypeAttributes.Public, typeof(ProxyBase));
            FieldBuilder realObjectField = typeBuilder.DefineField("realObject", interfaceType, FieldAttributes.Private);

            var ctor = EmitConstructor(typeBuilder, realObjectField);
            var createProxy = EmitCreateProxy(typeBuilder, ctor);

            var aspectFields = new List<AspectField>();

            foreach (var method in interfaceType.GetMethods(BindingFlags.Instance | BindingFlags.Public))
                EmitMethodSigil(typeBuilder, method, realObjectField, aspectFields);

            typeBuilder.AddInterfaceImplementation(interfaceType);
            var ret = typeBuilder.CreateType();

            foreach (var aspectField in aspectFields)
            {
                var field = ret.GetField(aspectField.Field.Name, BindingFlags.Static | BindingFlags.NonPublic);
                field.SetValue(null, aspectField.Aspect);
            }

            return ret;
        }

        private static ConstructorBuilder EmitConstructor(TypeBuilder typeBuilder, FieldBuilder realObjectField)
        {
            return Sigil.Emit<Action<object>>.BuildConstructor(typeBuilder, MethodAttributes.Public)
                    .LoadArgument(0)
                    .Call(typeBuilder.BaseType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null))
                    .LoadArgument(0)
                    .LoadArgument(1)
                    .CastClass(typeof(TInterfaceType))
                    .StoreField(realObjectField)
                    .Return()
                    .CreateConstructor();
        }

        private static MethodBuilder EmitCreateProxy(TypeBuilder typeBuilder, ConstructorBuilder ctor)
        {
            /* //CreateProxy cannot be created because of a limitation on Sigil
            var createProxy = Sigil.Emit<Func<TInterfaceType,TInterfaceType>>.BuildStaticMethod(typeBuilder, "CreateProxy", MethodAttributes.Public | MethodAttributes.Static, true, false)
                                .LoadArgument(0)
                                .NewObject(ctor)
                                .Return()
                                .CreateMethod();*/

            var createProxy = typeBuilder.DefineMethod("CreateProxy", MethodAttributes.Static | MethodAttributes.Public, interfaceType, new[] { interfaceType });
            ILGenerator il = createProxy.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Newobj, ctor);
            il.Emit(OpCodes.Ret);

            return createProxy;
        }

        private static void EmitMethodSigil(TypeBuilder typeBuilder, MethodInfo method, FieldBuilder realObjectField, List<AspectField> aspectFields)
        {
            var parameters = method.GetParameters();
            //var newMethod = typeBuilder.DefineMethod(method.Name, MethodAttributes.Public | MethodAttributes.Virtual, method.ReturnType, parameters.Select(i => i.ParameterType).ToArray());

            var emitter = Sigil.NonGeneric.Emit.BuildInstanceMethod(method.ReturnType, parameters.Select(i => i.ParameterType).ToArray(), typeBuilder, method.Name, MethodAttributes.Public | MethodAttributes.Virtual);

            var lbCallBase = emitter.DefineLabel();
            var lbReturn = emitter.DefineLabel();

            var methodContextLocal = emitter.DeclareLocal(typeof(MethodContext));
            var returnLocal = method.ReturnType != typeof(void) ? emitter.DeclareLocal(method.ReturnType) : null;
            var objArrayLocal = emitter.DeclareLocal(typeof(object[]));
            var attrs = method.GetCustomAttributes(typeof(AspectAttribute), false).Cast<AspectAttribute>().Concat(AspectFactory.GlobalAspects).ToList();

            string methodName = method.Name;
            if (attrs.Any())
            {
                Console.WriteLine(methodName);
                emitter
                    .LoadConstant(parameters.Length)
                    .NewArray<object>()
                    .StoreLocal(objArrayLocal); // var parameters = new object[parameters.Length];


                for (int i = 0; i < parameters.Length; i++)
                {
                    emitter
                        .LoadLocal(objArrayLocal)
                        .LoadConstant(i)
                        .LoadArgument((ushort)(i + 1))
                        .Box(parameters[i].ParameterType, true)
                        .StoreElement<object>(); // parameters[i] = paramN;
                }

                emitter
                     .LoadConstant(method)
                     .Call(typeof(MethodBase).GetMethod("GetMethodFromHandle", new[] { typeof(RuntimeMethodHandle) }))
                     .CastClass<MethodInfo>() // methodof(Method)

                     .LoadArgument(0)
                     .LoadField(realObjectField) // this.realObject

                     .LoadLocal(objArrayLocal)
                     .Call(typeof(ProxyBase).GetMethod("GetMethodContext", BindingFlags.Static | BindingFlags.NonPublic))
                     .StoreLocal(methodContextLocal); // methodContextLocal = GetMethodContext(methodof(Method), this.realObject, parameters
            }

            var currentMethodAspects = new List<AspectField>();
            foreach (var attr in attrs.OrderBy(i => i.Order))
            {
                var field = typeBuilder.DefineField("aspectField_" + Guid.NewGuid(), typeof(AspectAttribute), FieldAttributes.Static | FieldAttributes.Private);
                aspectFields.Add(new AspectField { Aspect = attr, Field = field });
                currentMethodAspects.Add(new AspectField { Aspect = attr, Field = field });

                emitter
                    .LoadField(field)
                    .LoadLocal(methodContextLocal)
                    .CallVirtual(typeof(AspectAttribute).GetMethod("InterceptStart", BindingFlags.Instance | BindingFlags.Public));


                if (method.ReturnType != typeof(void))
                {
                    emitter
                        .LoadLocal(methodContextLocal)
                        .Call(typeof(MethodContext).GetProperty("ReturnValue").GetGetMethod())
                        .BranchIfTrue(lbReturn);
                }
            }

            //Chama o método base
            emitter.MarkLabel(lbCallBase);

            if (attrs.Any() && method.ReturnType != typeof(void))  //Armazena o objeto retornado no ReturnValue caso haja aspectos
                emitter.LoadLocal(methodContextLocal);

            emitter
                .LoadArgument(0)
                .LoadField(realObjectField);
            for (int i = 0; i < parameters.Length; i++)
                emitter.LoadArgument((ushort)(i + 1));

            emitter.CallVirtual(method); //realObject.Method(parameters...);

            if (method.ReturnType != typeof(void))
                emitter.StoreLocal(returnLocal);


            //Armazena o objeto retornado no ReturnValue caso haja aspectos
            if (method.ReturnType != typeof(void) && attrs.Any())
            {
                emitter
                    .LoadLocal(returnLocal)
                    .Box(method.ReturnType, true)
                    .Call(typeof(MethodContext).GetProperty("ReturnValue").GetSetMethod());
            }

            emitter.MarkLabel(lbReturn);
            foreach (var aspects in currentMethodAspects.OrderByDescending(i => i.Aspect.Order))
            {
                emitter
                    .LoadField(aspects.Field)
                    .LoadLocal(methodContextLocal)
                    .CallVirtual(typeof(AspectAttribute).GetMethod("InterceptEnd", BindingFlags.Instance | BindingFlags.Public));
            }

            if (method.ReturnType != typeof(void))
            {
                if (attrs.Any())
                {
                    emitter
                        .LoadLocal(methodContextLocal)
                        .Call(typeof(MethodContext).GetProperty("ReturnValue").GetGetMethod())
                        .UnboxAny(method.ReturnType, true)
                        .StoreLocal(returnLocal);

                }

                if (method.ReturnType != typeof(void))
                    emitter.LoadLocal(returnLocal);
            }

            emitter.Return();

            var newMethod = emitter.CreateMethod();
            typeBuilder.DefineMethodOverride(newMethod, method);
        }

        private static void EmitMethod(TypeBuilder typeBuilder, MethodInfo method, FieldBuilder realObjectField, List<AspectField> aspectFields)
        {
            var parameters = method.GetParameters();
            var newMethod = typeBuilder.DefineMethod(method.Name, MethodAttributes.Public | MethodAttributes.Virtual, method.ReturnType, parameters.Select(i => i.ParameterType).ToArray());

            var il = newMethod.GetILGenerator();

            //il.Emit(OpCodes.Ldstr, "Method Invoked: " + method.Name);
            //il.EmitCall(OpCodes.Call, typeof(Debug).GetMethod("WriteLine", new[] { typeof(string) }), Type.EmptyTypes);

            var lbCallBase = il.DefineLabel();
            var lbReturn = il.DefineLabel();

            var methodContextLocal = il.DeclareLocal(typeof(MethodContext));
            var returnLocal = method.ReturnType != typeof(void) ? il.DeclareLocal(method.ReturnType) : null;
            var objArrayLocal = il.DeclareLocal(typeof(object[]));
            var attrs = method.GetCustomAttributes(typeof(AspectAttribute), false).Cast<AspectAttribute>().Concat(AspectFactory.GlobalAspects).ToList();

            if (attrs.Any())
            {
                il.Emit(OpCodes.Ldtoken, method);
                il.Emit(OpCodes.Call, typeof(MethodBase).GetMethod("GetMethodFromHandle", new[] { typeof(RuntimeMethodHandle) }));
                il.Emit(OpCodes.Castclass, typeof(MethodInfo)); //MethodInfo

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, realObjectField); //this.realObject

                il.Emit(OpCodes.Ldc_I4, parameters.Length);
                il.Emit(OpCodes.Newarr, typeof(object));
                il.Emit(OpCodes.Stloc, objArrayLocal.LocalIndex); //var parameters = new object[parameters.Length];

                for (int i = 0; i < parameters.Length; i++)
                {
                    il.Emit(OpCodes.Ldloc, objArrayLocal.LocalIndex);
                    il.Emit(OpCodes.Ldc_I4, i);
                    il.Emit(OpCodes.Ldarg, (i + 1));
                    if (parameters[i].ParameterType.IsValueType)
                        il.Emit(OpCodes.Box, parameters[i].ParameterType);
                    il.Emit(OpCodes.Stelem_Ref); // parameters[i] = paramN;
                }

                il.Emit(OpCodes.Ldloc, objArrayLocal.LocalIndex);
                il.EmitCall(OpCodes.Call, typeof(ProxyBase).GetMethod("GetMethodContext", BindingFlags.Static | BindingFlags.NonPublic), Type.EmptyTypes);
                il.Emit(OpCodes.Stloc, methodContextLocal.LocalIndex); //methodContextLocal = GetMethodContext(parameters);
            }

            var currentMethodAspects = new List<AspectField>();
            foreach (var attr in attrs.OrderBy(i => i.Order))
            {
                var field = typeBuilder.DefineField("aspectField_" + Guid.NewGuid(), typeof(AspectAttribute), FieldAttributes.Static | FieldAttributes.Private);
                aspectFields.Add(new AspectField { Aspect = attr, Field = field });
                currentMethodAspects.Add(new AspectField { Aspect = attr, Field = field });

                il.Emit(OpCodes.Ldsfld, field);
                il.Emit(OpCodes.Ldloc, methodContextLocal.LocalIndex);
                il.EmitCall(OpCodes.Callvirt, typeof(AspectAttribute).GetMethod("InterceptStart", BindingFlags.Instance | BindingFlags.Public), Type.EmptyTypes);
                if (method.ReturnType != typeof(void))
                {
                    il.Emit(OpCodes.Ldloc, methodContextLocal.LocalIndex);
                    il.EmitCall(OpCodes.Call, typeof(MethodContext).GetProperty("ReturnValue").GetGetMethod(), Type.EmptyTypes);
                    il.Emit(OpCodes.Brtrue, lbReturn);
                }
            }

            //Chama o método base
            il.MarkLabel(lbCallBase);

            if (attrs.Any() && method.ReturnType != typeof(void))  //Armazena o objeto retornado no ReturnValue caso haja aspectos
                il.Emit(OpCodes.Ldloc, methodContextLocal.LocalIndex);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, realObjectField);
            for (int i = 0; i < parameters.Length; i++)
                il.Emit(OpCodes.Ldarg, (i + 1));
            il.EmitCall(OpCodes.Callvirt, method, Type.EmptyTypes);
            if (method.ReturnType != typeof(void))
            {
                il.Emit(OpCodes.Stloc, returnLocal.LocalIndex);
                //il.Emit(OpCodes.Pop);
            }

            //Armazena o objeto retornado no ReturnValue caso haja aspectos
            if (method.ReturnType != typeof(void) && attrs.Any())
            {
                il.Emit(OpCodes.Ldloc, returnLocal.LocalIndex);
                if (method.ReturnType.IsValueType)
                    il.Emit(OpCodes.Box, method.ReturnType);

                il.EmitCall(OpCodes.Call, typeof(MethodContext).GetProperty("ReturnValue").GetSetMethod(), Type.EmptyTypes);
            }

            il.MarkLabel(lbReturn);
            foreach (var aspects in currentMethodAspects.OrderByDescending(i => i.Aspect.Order))
            {
                il.Emit(OpCodes.Ldsfld, aspects.Field);
                il.Emit(OpCodes.Ldloc, methodContextLocal.LocalIndex);
                il.EmitCall(OpCodes.Callvirt, typeof(AspectAttribute).GetMethod("InterceptEnd", BindingFlags.Instance | BindingFlags.Public), Type.EmptyTypes);
            }

            if (method.ReturnType != typeof(void))
            {
                if (attrs.Any())
                {
                    il.Emit(OpCodes.Ldloc, methodContextLocal.LocalIndex);
                    il.EmitCall(OpCodes.Call, typeof(MethodContext).GetProperty("ReturnValue").GetGetMethod(), Type.EmptyTypes);

                    if (method.ReturnType.IsValueType)
                        il.Emit(OpCodes.Unbox, method.ReturnType);
                    il.Emit(OpCodes.Stloc, returnLocal.LocalIndex);
                }

                if (method.ReturnType != typeof(void))
                    il.Emit(OpCodes.Ldloc, returnLocal.LocalIndex);
            }

            il.Emit(OpCodes.Ret);
            typeBuilder.DefineMethodOverride(newMethod, method);
        }

        private class AspectField
        {
            public AspectAttribute Aspect { get; set; }
            public FieldInfo Field { get; set; }
        }
    }
}
