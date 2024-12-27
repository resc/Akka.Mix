using Akka.Actor;
using Akka.Cluster.Tools.PublishSubscribe;
using Akka.Event;
using Akka.Mix.Messages;

namespace Akka.Mix.Leader;

public class CountReportProcessor : ReceiveActor, IWithTimers
{
    public ITimerScheduler Timers { get; set; } = null!;
    private readonly CountReportCache _cache = new();

    public CountReportProcessor()
    {
        _mediator = DistributedPubSub.Get(Context.System).Mediator;
        _hello = new SendHello(new CountTopicHello { Message = "Hello from " + Self.Path.Name });
        var log = Context.GetLogger();
        log.Info("CountReportProcessor started");
        Receive<CountReport>(countReport =>
        {
            log.Info($"Received count report {countReport} from {Sender}");
            if (_cache.TryAdd(countReport))
            {
                log.Info($"Processing count report {countReport}...");
                // TODO: process the count report                
            }
            else
            {
                var error = $"Received already known count report {countReport} from {Sender}, not processing...";
                log.Warning(error);
                // tell the reporter we've noticed something funny about the report
                // We'll ack the report any, but not process it
                Sender.Tell(new NackCountReport
                {
                    Reporter = countReport.Reporter,
                    SequenceNumber = countReport.SequenceNumber,
                    Errors = { error }
                });
            }

            // tell the reporter we've handled the report
            Sender.Tell(new AckCountReport
            {
                Reporter = countReport.Reporter,
                SequenceNumber = countReport.SequenceNumber
            });


        });
        
        Receive<SubscribeAck>(msg =>
        {
            log.Info($"CountReportProcessor received SubscribeAck {msg.Subscribe.Topic}");
        });

        Receive<CountTopicHello>(hello =>
        {
            log.Warning("CountTopicHello received from {0}", Sender);
        });

        Receive<SendHello>(hello =>
        {
            _mediator.Tell(new Publish(CountReport.Topic, hello.Hello));
        });
    }

    private readonly IActorRef _mediator;
    private readonly SendHello _hello;

    private record SendHello(CountTopicHello Hello);
 
    protected override void PreStart()
    {
        _mediator.Tell(new Put(Self));
        _mediator.Tell(new Subscribe(CountReport.Topic, Self));
        Timers.StartPeriodicTimer(_hello, _hello, TimeSpan.FromSeconds(10));
    }



}
