using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Aspects
{
    /// <summary>
    /// Default cache aspect implementation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public abstract class CacheAspectAttribute : AspectAttribute
    {
        /// <summary>
        /// Defines order to interception. Lower order gets InterceptStart executed earlier, while InterceptEnd is executed later.
        /// </summary>
        protected internal override int Order { get { return int.MinValue + 1; } }

        /// <summary>
        /// Method executed before real method execution. If ReturnValue is set, the real method body is not executed.
        /// </summary>
        /// <param name="method"></param>
        public override sealed void InterceptStart(Simple.MethodContext method)
        {
            string key = GetKey(method);
            method["Cache.Key"] = key;
            method.ReturnValue = this.GetObject(key);
            method["Cache.Hit"] = method.ReturnValue != null;
        }

        /// <summary>
        /// Method executed after real method execution.
        /// </summary>
        /// <param name="method"></param>
        public override sealed void InterceptEnd(MethodContext method)
        {
            string key = (string)method["Cache.Key"];

            bool cacheHit = (bool)method["Cache.Hit"];
            if (cacheHit)
                this.TouchObject(key, method.ReturnValue);
            else
                this.StoreObject(key, method.ReturnValue);
        }

        /// <summary>
        /// Get key for a method.
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        protected virtual string GetKey(MethodContext method)
        {
            StringBuilder sb = new StringBuilder();
            sb
                .Append(method.Method.DeclaringType.Name)
                .Append(".")
                .Append(method.Method.Name);
            foreach (var param in method.Parameters)
                sb.Append("|").Append(param.Name).Append(":").Append(param.GetStringValue());

            return sb.ToString();
        }

        /// <summary>
        /// Gets object from defined cache storage. Should be implemented in derived class.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected abstract object GetObject(string key);

        /// <summary>
        /// Sets object from defined cache storage. Should be implemented in derived class.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        protected abstract void StoreObject(string key, object obj);

        /// <summary>
        /// When implemented, allows to define new expiration date.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        protected virtual void TouchObject(string key, object obj)
        {

        }
    }
}
