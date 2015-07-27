using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Simple.Aspects
{
    /// <summary>
    /// Default log aspect implementation.
    /// </summary>
    public abstract class LogAspectAttribute : AspectAttribute
    {
        /// <summary>
        /// Defines order to interception. Lower priority gets MethodEnter called earlier.
        /// </summary>
        protected internal override int EnterPriority { get { return int.MinValue; } }

        /// <summary>
        /// Method executed before real method execution. If ReturnValue is set, the real method body is not executed.
        /// </summary>
        /// <param name="method"></param>
        public override sealed void MethodEnter(Simple.MethodContext method)
        {
            method.SetValue("sw", Stopwatch.StartNew());
        }

        /// <summary>
        /// Method executed after real method execution.
        /// </summary>
        /// <param name="method"></param>
        public override sealed void MethodExit(MethodContext method)
        {
            var sw = method.GetValue("sw") as Stopwatch;
            var logEntry = new LogEntry(method, sw.Elapsed, method.ReturnValue);
            this.InsertLog(logEntry);
        }

        /// <summary>
        /// Method for implemententing log inserting. Should be implemented in derived class.
        /// </summary>
        /// <param name="logEntry"></param>
        protected abstract void InsertLog(LogEntry logEntry);

        #region LogEntry
        /// <summary>
        /// Defines a log entry.
        /// </summary>
        public class LogEntry
        {
            /// <summary>
            /// Log Entry
            /// </summary>
            /// <param name="method"></param>
            /// <param name="elapsed"></param>
            /// <param name="returnValue"></param>
            public LogEntry(MethodContext method, TimeSpan elapsed, object returnValue)
            {
                this.Method = method;
                this.Elapsed = elapsed;
                this.ReturnValue = returnValue;
            }

            /// <summary>
            /// Method information.
            /// </summary>
            public MethodContext Method { get; private set; }
            /// <summary>
            /// Elapsed time.
            /// </summary>
            public TimeSpan Elapsed { get; private set; }
            /// <summary>
            /// The returned object (null if method is void).
            /// </summary>
            public object ReturnValue { get; private set; }
        }
        #endregion LogEntry
    }
}
