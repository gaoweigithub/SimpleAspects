using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
    internal static class ExtensionMethods
    {
        public static Sigil.NonGeneric.Emit Unbox(this Sigil.NonGeneric.Emit emitter, Type typeTo, bool castIfRefType)
        {
            if (!typeTo.IsValueType)
                return emitter.CastClass(typeTo);

            return emitter.Unbox(typeTo);
        }

        public static Sigil.NonGeneric.Emit UnboxAny(this Sigil.NonGeneric.Emit emitter, Type typeTo, bool castIfRefType)
        {
            if (!typeTo.IsValueType)
                return emitter.CastClass(typeTo);

            return emitter.UnboxAny(typeTo);
        }

        public static Sigil.NonGeneric.Emit Box(this Sigil.NonGeneric.Emit emitter, Type typeFrom, bool castIfRefType)
        {
            if (typeFrom.IsByRef)
            {
                var baseType = typeFrom.GetElementType();

                if( baseType.IsPrimitive)
                    return emitter.LoadIndirect(baseType).Box(baseType);

                if( baseType.IsValueType)
                    return emitter.LoadObject(baseType).Box(baseType);

                if (!baseType.IsValueType)
                    return emitter.LoadIndirect(baseType);

                throw new NotSupportedException(typeFrom.FullName + " not supported in this context");
            }

            if (!typeFrom.IsValueType)
                return emitter.CastClass(typeFrom);

            return emitter.Box(typeFrom);
        }
    }
}
