using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Simple
{
    /// <summary>
    /// Base aspect attribute
    /// </summary>
    public class AspectAttribute : Attribute
    {
        /// <summary>
        /// Defines order to interception. Lower order gets InterceptStart executed earlier, while InterceptEnd is executed later.
        /// </summary>
        internal protected virtual int Order { get { return 0; } }

        /// <summary>
        /// Method executed before real method execution. If ReturnValue is set, the real method body is not executed.
        /// </summary>
        /// <param name="method"></param>
        public virtual void InterceptStart(MethodContext method)
        {
        }

        /// <summary>
        /// Method executed after real method execution.
        /// </summary>
        /// <param name="method"></param>
        public virtual void InterceptEnd(MethodContext method)
        {
        }
    }
}
