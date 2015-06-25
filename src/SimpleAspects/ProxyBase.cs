using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Simple
{
    /// <summary>
    /// Not meant to be used.
    /// </summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public abstract class ProxyBase
    {
        /// <summary>
        /// GetMethodContext
        /// </summary>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected static MethodContext GetMethodContext(MethodInfo method, object[] parameters)
        {
            var methodParameters = method.GetParameters().Select((param, idx) => new MethodParameter(param, parameters[idx])).ToList();
            return new MethodContext(method, methodParameters);
        }
    }
}