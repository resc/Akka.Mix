using Akka.Actor;
using Akka.DependencyInjection;

namespace Akka.Mix.Net48;

/// <summary> Manages a collection of count reporters. </summary>
public class CountReporterManager : ReceiveActor
{
    public CountReporterManager()
    {
        // start all count reporters
        var resolver = DependencyResolver.For(Context.System);
        var names = new[] { "IN01", "OUT1", "OUT2" };
        foreach (var name in names)
        {
            var props = resolver.Props<CountReporter>(name);
            Context.ActorOf(props, name);
        }
    }
}
