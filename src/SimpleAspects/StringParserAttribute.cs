using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple
{
    /// <summary>
    /// Defines a converter to string for a parameter value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public abstract class StringParserAttribute : Attribute
    {
        /// <summary>
        /// Returns string for a value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public abstract string GetStringValue(object value);
    }
}
