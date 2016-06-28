# SimpleAspects
A simple AOP framework

Usage:

Interface:
public interface IUserService
{
    [LogPerformanceAspect]
    User GetById(Guid id);
}

Aspect:
public class LogPerformanceAspectAttribute : AspectAttribute
{
    public override void MethodEnter(MethodContext method)
    {
        //The aspect attribute is a singleton, so we should not store information in itself.
        //Instead we store in MethodContext instance
        
        var sw = System.Diagnostics.Stopwatch.Start();
        method["LogPerformance.Stopwatch"] = sw;
    }

    public override void MethodExit(MethodContext method)
    {
        var sw = (System.Diagnostics.Stopwatch)method["LogPerformance.Stopwatch"];
        Console.WriteLine("Method '{0}' took {1} seconds.", method.Method.Name, sw.Elapsed.TotalSeconds);
    }
}

How proxies are created:
var userService = AspectFactory.Create<IUserService>(new UserService()); 
//UserService here is the real implementation, which aspects would be applied to.
//This can be used in DI container.
