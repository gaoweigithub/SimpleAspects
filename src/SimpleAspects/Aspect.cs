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
        /// Defines order to interception. Lower priority gets MethodEnter called earlier.
        /// </summary>
        internal protected virtual int EnterPriority { get { return 0; } }
        /// <summary>
        /// Defines order to interception. Lower priority gets MethodExit called earlier.
        /// </summary>
        internal protected virtual int ExitPriority { get { return 0; } }

        /// <summary>
        /// Method called before real method execution. If ReturnValue is set, the real method body is not executed.
        /// </summary>
        /// <param name="method"></param>
        public virtual void MethodEnter(MethodContext method)
        {
        }

        /// <summary>
        /// Method called after real method execution.
        /// </summary>
        /// <param name="method"></param>
        public virtual void MethodExit(MethodContext method)
        {
        }

        /// <summary>
        /// Method called when an exception occurs.
        /// </summary>
        /// <param name="exception"></param>
        public virtual void ExceptionFilter(Exception exception)
        {

        }
    }
}
