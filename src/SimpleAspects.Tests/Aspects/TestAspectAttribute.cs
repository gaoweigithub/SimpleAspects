using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Tests
{
    class TestAspectAttribute : AspectAttribute
    {
        public static Action<MethodContext> InterceptStartCallback;
        public static Action<MethodContext> InterceptEndCallback;

        public override void InterceptStart(MethodContext method)
        {
            if (InterceptStartCallback != null)
                InterceptStartCallback(method);
        }

        public override void InterceptEnd(MethodContext method)
        {
            if (InterceptEndCallback != null)
                InterceptEndCallback(method);
        }

        public static void ClearCallbacks()
        {
            InterceptStartCallback = null;
            InterceptEndCallback = null;
        }
    }

    class TestAspect2Attribute : AspectAttribute
    {
        public static Action<MethodContext> InterceptStartCallback;
        public static Action<MethodContext> InterceptEndCallback;

        public override void InterceptStart(MethodContext method)
        {
            if (InterceptStartCallback != null)
                InterceptStartCallback(method);
        }

        public override void InterceptEnd(MethodContext method)
        {
            if (InterceptEndCallback != null)
                InterceptEndCallback(method);
        }

        public static void ClearCallbacks()
        {
            InterceptStartCallback = null;
            InterceptEndCallback = null;
        }
    }
}
