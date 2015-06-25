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
        public static readonly ModuleBuilder Instance = CreateModule();

        private static ModuleBuilder CreateModule()
        {
            AppDomain myDomain = Thread.GetDomain();
            AssemblyName myAsmName = new AssemblyName();
            myAsmName.Name = "SimpleProxies.Assembly";

            AssemblyBuilder asmBuilder = myDomain.DefineDynamicAssembly(myAsmName, AssemblyBuilderAccess.Run);

            ModuleBuilder module = asmBuilder.DefineDynamicModule("SimpleProxies.Module");
            return module;
        }
    }
}
