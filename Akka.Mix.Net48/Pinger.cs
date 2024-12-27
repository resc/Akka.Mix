using Akka.Actor;
using Akka.Event;
using Akka.Mix.Messages;

namespace Akka.Mix.Net48;

public class Pinger : ReceiveActor, IWithTimers
{
    public ITimerScheduler Timers { get; set; } = null!;

    public Pinger()
    {
        var log = Context.GetLogger();
        log.Info("Pinger started");
        Timers.StartPeriodicTimer("ping", "ping", TimeSpan.FromSeconds(3));
        Receive<string>(message =>
        {
            switch (message)
            {
                case "ping":
                    log.Info("Pinging ponger from {0}", Sender.Path);
                    var ping = new Ping { Message = $"ping {DateTime.Now:T}" };
                    Context.ActorSelection("akka.tcp://Akka-Mix@localhost:8100/user/ponger").Tell(ping);
                    break;
                default:
                    log.Info("received unhandled message: {0}", message);
                    break;
            }
        });

        Receive<Pong>(pong =>
        {
            log.Info("received pong ({1}) from {0}", Sender.Path, pong.Message);
        });
    }
}
