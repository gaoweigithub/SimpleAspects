using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Simple.Aspects;
using System.Diagnostics;
using System.Linq;

namespace Simple.Tests
{
    [TestClass]
    public class AOPTests
    {
        [TestCleanup]
        public void CleanUp()
        {
            TestAspectAttribute.ClearCallbacks();
            TestAspect2Attribute.ClearCallbacks();
            AspectFactory.ClearGlobalAspects();
        }


        [TestMethod]
        public void ShouldExecuteGlobalAspects()
        {
            MethodContext startContext = null;
            MethodContext endContext = null;

            TestAspectAttribute.InterceptStartCallback = (ctx) => startContext = ctx;
            TestAspectAttribute.InterceptEndCallback = (ctx) => endContext = ctx;
            AspectFactory.RegisterGlobalAspect(new TestAspectAttribute());

            var proxy = AspectFactory.Create<IParameterPassingTest>(new ParameterPassingTest());

            proxy.Test3();

            Assert.IsNotNull(startContext, "InterceptStart was called.");
            Assert.IsNotNull(endContext, "InterceptEnd was called.");
        }


        [TestMethod]
        public void ShouldImplementCacheAspect()
        {
            var realRepository = new UserRepository();
            var repository = AspectFactory.Create<IUserRepository>(realRepository);

            var user = new User { Id = Guid.NewGuid(), Name = "User 1" };
            repository.Save(user);

            var u1 = repository.GetById(user.Id);
            var u2 = repository.GetById(user.Id);
            var u3 = repository.GetById(user.Id);

            Assert.AreEqual(1, realRepository.GetByIdCount);

        }


        [TestMethod]
        public void ParametersShouldBePassedProperly()
        { 
            MethodContext startContext = null;
            MethodContext endContext = null;

            TestAspectAttribute.InterceptStartCallback = (ctx) => startContext = ctx;
            TestAspectAttribute.InterceptEndCallback = (ctx) => endContext = ctx;

            var obj = new ParameterPassingTest(5, "test", DateTime.Now.Subtract(TimeSpan.FromMinutes(9999)), long.MaxValue, true, Enumerable.Range(10, 5).ToArray());

            var proxy = AspectFactory.Create<IParameterPassingTest>(obj);

            bool ok = proxy.Test(obj.P1, obj.P2, obj.P3, obj.P4, obj.P5, obj.P6);

            Assert.IsTrue(ok, "Parameters was ok on real object.");
            Assert.IsNotNull(startContext, "InterceptStart was called.");
            Assert.IsNotNull(endContext, "InterceptEnd was called.");        
    
            var parameters = new object[]{obj.P1, obj.P2, obj.P3, obj.P4, obj.P5, obj.P6};
            bool okInterceptParameters = startContext.Parameters.Select(i => i.Value).SequenceEqual(parameters);
            Assert.IsTrue(okInterceptParameters, "Parameters received in intercept method was ok on aspect.");
        }


        [TestMethod]
        public void MultipleAspectShouldIntercept()
        {
            MethodContext startContext1, startContext2, endContext1, endContext2;
            startContext1 = startContext2 = endContext1 = endContext2 = null;            

            TestAspectAttribute.InterceptStartCallback = (ctx) => startContext1 = ctx;
            TestAspectAttribute.InterceptEndCallback = (ctx) => endContext1 = ctx;
            TestAspect2Attribute.InterceptStartCallback = (ctx) => startContext2 = ctx;
            TestAspect2Attribute.InterceptEndCallback = (ctx) => endContext2 = ctx;

            var obj = new ParameterPassingTest();

            var proxy = AspectFactory.Create<IParameterPassingTest>(obj);

            proxy.Test2("test", 99);

            Assert.IsNotNull(startContext1, "InterceptStart1 was called.");
            Assert.IsNotNull(endContext1, "InterceptEnd1 was called.");
            Assert.IsNotNull(startContext2, "InterceptStart2 was called."); 
            Assert.IsNotNull(endContext2, "InterceptEnd2 was called.");
        }

    }
}
