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
                EmitMethod(typeBuilder, method, realObjectField, aspectFields);

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
            var createProxy = Sigil.Emit<Func<TInterfaceType,TInterfaceType>>.BuildStaticMethod(typeBuilder, "CreateProxy", MethodAttributes.Public | MethodAttributes.Static, true, false)
                                .LoadArgument(0)
                                .NewObject(ctor, Type.EmptyTypes)
                                .Return()
                                .CreateMethod();

            return createProxy;
        }

        private static void EmitMethod(TypeBuilder typeBuilder, MethodInfo method, FieldBuilder realObjectField, List<AspectField> aspectFields)
        {
            var parameters = method.GetParameters();

            var emitter = Sigil.NonGeneric.Emit.BuildInstanceMethod(method.ReturnType, parameters.Select(i => i.ParameterType).ToArray(), typeBuilder, method.Name, MethodAttributes.Public | MethodAttributes.Virtual);

            var lbCallBase = emitter.DefineLabel();
            var lbReturn = emitter.DefineLabel();

            var methodContextLocal = emitter.DeclareLocal(typeof(MethodContext));
            var returnLocal = method.ReturnType != typeof(void) ? emitter.DeclareLocal(method.ReturnType) : null;
            var objArrayLocal = emitter.DeclareLocal(typeof(object[]));
            var aspects = method.GetCustomAttributes(typeof(AspectAttribute), false).Cast<AspectAttribute>().Concat(AspectFactory.GlobalAspects).ToList();

            bool hasException = aspects.Any(i => i.GetType().GetMethod("ExceptionFilter").DeclaringType != typeof(AspectAttribute));

            Sigil.ExceptionBlock tryBlock = null;
            if (hasException)
                tryBlock = emitter.BeginExceptionBlock();

            string methodName = method.Name;
            if (aspects.Any())
            {
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
            foreach (var aspect in aspects.OrderBy(i => i.EnterPriority))
            {
                var field = typeBuilder.DefineField("aspectField_" + Guid.NewGuid(), typeof(AspectAttribute), FieldAttributes.Static | FieldAttributes.Private);
                aspectFields.Add(new AspectField { Aspect = aspect, Field = field });
                currentMethodAspects.Add(new AspectField { Aspect = aspect, Field = field });

                emitter
                    .LoadField(field)
                    .LoadLocal(methodContextLocal)
                    .CallVirtual(typeof(AspectAttribute).GetMethod(GetName(aspect.MethodEnter), BindingFlags.Instance | BindingFlags.Public));


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

            if (aspects.Any() && method.ReturnType != typeof(void))  //Armazena o objeto retornado no ReturnValue caso haja aspectos
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
            if (method.ReturnType != typeof(void) && aspects.Any())
            {
                emitter
                    .LoadLocal(returnLocal)
                    .Box(method.ReturnType, true)
                    .Call(typeof(MethodContext).GetProperty("ReturnValue").GetSetMethod());
            }

            if (hasException)
            {
                var catchBlock = emitter.BeginCatchBlock<Exception>(tryBlock);
                var exceptionLocal = emitter.DeclareLocal<Exception>();
                emitter.StoreLocal(exceptionLocal);

                foreach (var aspect in currentMethodAspects.Where(i => i.Aspect.GetType().GetMethod("ExceptionFilter").DeclaringType != typeof(AspectAttribute)))
                    emitter
                        .LoadField(aspect.Field)
                        .LoadLocal(exceptionLocal)
                        .CallVirtual(typeof(AspectAttribute).GetMethod("ExceptionFilter", BindingFlags.Instance | BindingFlags.Public)); ;

                emitter
                    .ReThrow()
                    .EndCatchBlock(catchBlock)
                    .EndExceptionBlock(tryBlock);
            }


            emitter.MarkLabel(lbReturn);
            foreach (var aspect in currentMethodAspects.OrderBy(i => i.Aspect.ExitPriority))
            {
                emitter
                    .LoadField(aspect.Field)
                    .LoadLocal(methodContextLocal)
                    .CallVirtual(typeof(AspectAttribute).GetMethod(GetName(aspect.Aspect.MethodExit), BindingFlags.Instance | BindingFlags.Public));
            }

            if (method.ReturnType != typeof(void))
            {
                if (aspects.Any())
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

        private static string GetName(Action<MethodContext> action)
        {
            return action.Method.Name;
        }

        private static string GetName(Action<Exception> action)
        {
            return action.Method.Name;
        }

        private class AspectField
        {
            public AspectAttribute Aspect { get; set; }
            public FieldInfo Field { get; set; }
        }
    }
}
