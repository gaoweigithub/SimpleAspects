using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;

namespace Simple
{
    internal class SimpleModuleBuilder
    {
        public static ModuleBuilder Instance { get; private set; }

        static SimpleModuleBuilder()
        {
            ClearCache();
        }

        internal static void ClearCache()
        {
            AppDomain myDomain = Thread.GetDomain();
            AssemblyName myAsmName = new AssemblyName();
            myAsmName.Name = "SimpleProxies.Assembly";

            AssemblyBuilder asmBuilder = myDomain.DefineDynamicAssembly(myAsmName, AssemblyBuilderAccess.RunAndSave);
            Instance = asmBuilder.DefineDynamicModule("SimpleProxies.Module");
        }
    }
}