using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple
{
    /// <summary>
    /// AOP main component
    /// </summary>
    public class AspectFactory
    {
        internal static readonly IList<AspectAttribute> GlobalAspects = new List<AspectAttribute>();

        private static Dictionary<Type, Func<object>> container = new Dictionary<Type, Func<object>>();

        /// <summary>
        /// Gets a delegate for proxy creation.
        /// </summary>
        /// <typeparam name="TInterfaceType"></typeparam>
        /// <returns></returns>
        public static Func<TInterfaceType, TInterfaceType> GetBuilder<TInterfaceType>()
        {
            return ProxyBuilder<TInterfaceType>.ObjectBuilder;
        }

        /// <summary>
        /// Creates an proxy for an object.
        /// </summary>
        /// <typeparam name="TInterfaceType"></typeparam>
        /// <param name="realObject"></param>
        /// <returns></returns>
        public static TInterfaceType Create<TInterfaceType>(TInterfaceType realObject)
        {
            return Create(() => realObject);
        }
        /// <summary>
        /// Creates an proxy for an object.
        /// </summary>
        /// <typeparam name="TInterfaceType"></typeparam>
        /// <typeparam name="TBaseType"></typeparam>
        /// <returns></returns>
        public static TInterfaceType Create<TInterfaceType, TBaseType>()
           where TBaseType : TInterfaceType, new()
        {
            return Create<TInterfaceType>(() => new TBaseType());
        }
        /// <summary>
        /// Creates an proxy for an object.
        /// </summary>
        /// <typeparam name="TInterfaceType"></typeparam>
        /// <param name="realObjecteBuilder"></param>
        /// <returns></returns>
        public static TInterfaceType Create<TInterfaceType>(Func<TInterfaceType> realObjecteBuilder)
        {
            return ProxyBuilder<TInterfaceType>.Create(realObjecteBuilder());
        }

        /// <summary>
        /// Register a global aspect.
        /// </summary>
        /// <param name="aspect"></param>
        public static void RegisterGlobalAspect(AspectAttribute aspect)
        {
            GlobalAspects.Add(aspect);
        }

        /// <summary>
        /// Clear all global aspect.
        /// </summary>
        public static void ClearGlobalAspects()
        {
            GlobalAspects.Clear();
        }
    }
}