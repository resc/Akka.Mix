using Akka.Actor;
using Akka.Event;
using Akka.Mix.Messages;

namespace Akka.Mix.Leader;

public class Ponger : ReceiveActor
{
    public Ponger()
    {
        var log = Context.GetLogger();
        log.Info("Ponger started");

        Receive<Ping>(ping =>
        {
            log.Info("Received ping '{1}' from {0}", Sender.Path, ping.Message);
            Sender.Tell(new Pong { Message = ping.Message });
        });
    }
}
