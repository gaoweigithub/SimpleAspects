using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Simple
{
    /// <summary>
    /// Defines a method execution.
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough]
    public class MethodContext
    {
        /// <summary>
        /// MethodContext
        /// </summary>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <param name="realObject"></param>
        public MethodContext(MethodInfo method, object realObject, IList<MethodParameter> parameters)
        {
            this.RealObject = realObject;
            this.Method = method;
            this.Parameters = parameters;
        }

        /// <summary>
        /// MethodInfo invoked.
        /// </summary>
        public MethodInfo Method { get; private set; }
        /// <summary>
        /// Real type of.
        /// </summary>
        public object RealObject { get; private set; }
        /// <summary>
        /// Parameters of invokation.
        /// </summary>
        public IList<MethodParameter> Parameters { get; private set; }
        /// <summary>
        /// Value returned from base implementation (or overrided).
        /// </summary>
        public object ReturnValue { get; set; }

        Dictionary<string, object> properties = new Dictionary<string, object>();

        /// <summary>
        /// Sets a property value.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="obj"></param>
        public void SetValue(string property, object obj)
        {
            properties.Add(property, obj);
        }

        /// <summary>
        /// Gets the value for property name.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public object GetValue(string property)
        {
            object ret;
            if (properties.TryGetValue(property, out ret))
                return ret;

            return null;
        }

        /// <summary>
        /// Gets or sets custom properties.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public object this[string property]
        {
            get { return this.GetValue(property); }
            set { this.SetValue(property, value); }
        }
    }
}
