using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Simple
{
    /// <summary>
    /// Defines a method parameters.
    /// </summary>
    public class MethodParameter
    {
        /// <summary>
        /// MethodParameter
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="value"></param>
        public MethodParameter(ParameterInfo parameter, object value)
        {
            this.Parameter = parameter;
            this.Value = value;
        }

        /// <summary>
        /// ParameterInfo of the parameter.
        /// </summary>
        public ParameterInfo Parameter { get; private set; }
        /// <summary>
        /// Parameter's name.
        /// </summary>
        public string Name { get { return this.Parameter.Name; } }
        /// <summary>
        /// Parameter's value.
        /// </summary>
        public object Value { get; private set; }

        /// <summary>
        /// Parse the value to string (can use StringParserAttribute, if defined).
        /// </summary>
        /// <returns></returns>
        public string GetStringValue()
        {
            if (this.Value == null)
                return "[[null]]";
            var attr = this.Parameter.GetCustomAttributes(typeof(StringParserAttribute), true).Cast<StringParserAttribute>().FirstOrDefault();
            if (attr == null)
                return this.Value.ToString();

            return attr.GetStringValue(this.Value);
        }
    }
}
