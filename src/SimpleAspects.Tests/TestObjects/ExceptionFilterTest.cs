using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Tests
{
    public interface IExceptionFilterTest
    {
        [ExceptionFilterAspect]
        void RaiseException();
    }

    public class ExceptionFilterTest : IExceptionFilterTest
    {

        public void RaiseException()
        {
            throw new Exception("Teste");
        }
    }
}
