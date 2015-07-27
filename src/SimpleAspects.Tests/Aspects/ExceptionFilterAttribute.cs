using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Tests
{
    class ExceptionFilterAspect : AspectAttribute
    {
        public static event Action<Exception> DefaultHandler; 

        public override void ExceptionFilter(Exception exception)
        {
            if (DefaultHandler != null)
                DefaultHandler(exception);

            base.ExceptionFilter(exception);
        }
    }
}
