using System.Diagnostics;
using Akka.Actor;
using Akka.Cluster.Tools.PublishSubscribe;
using Akka.Event;
using Akka.Mix.Messages;

namespace Akka.Mix.Follower;

/// <summary>
/// A count reporter is an actor that reports the number of items picked or placed by a robot.
/// This reporter would maintain a connection with the robot hardware. 
/// The robot would wait on a sequence number acknowledgement before proceding to move the next batch of items.
public class CountReporter : ReceiveActor, IWithTimers
{
    private static readonly object _nextReportTimerKey = new object();
    private ProtocolState _state = new ProtocolState("", 0, 0, 0);
    private readonly ILoggingAdapter _log;
    public ITimerScheduler Timers { get; set; } = null!;

    private readonly IActorRef _mediator;

    public CountReporter(string CountReporter)
    {
        _mediator = DistributedPubSub.Get(Context.System).Mediator;
        _log = Context.GetLogger();
        if (string.IsNullOrWhiteSpace(CountReporter))
        {
            throw new ArgumentException($"'{nameof(CountReporter)}' cannot be null or whitespace.", nameof(CountReporter));
        }
        _state = _state with { Reporter = CountReporter };

        _log.Info("CountReporterProtocol started for {0}", CountReporter);

        Receive<AckCountReport>(ack =>
            {
                _state = _state.AckSequenceNumber(ack);
                _log.Info("Ack received for plant report {0}", _state);
                ScheduleNextCountReport();
            },
            ack => _state.CanBeAckedBy(ack));

        Receive<NackCountReport>(nack =>
            {
                var errors = string.Join(", ", nack.Errors);
                if (errors.Length == 0) errors = $"No errors specified in Nack {nack}";
                _log.Error("Nack received for plant report {0}: {1}", _state, errors);
                ScheduleNextCountReport();
            }, nack => _state.CanBeNackedBy(nack));

        Receive<NextCountReport>(_ => PublishCountReport());
    }


    protected override void PreStart()
    {
        base.PreStart();
        ScheduleNextCountReport();
    }

    private void ScheduleNextCountReport()
    {
        var delay = new Random(Stopwatch.GetTimestamp().GetHashCode()).NextDouble() * 4 + 2;
        Timers.StartPeriodicTimer(_nextReportTimerKey, new NextCountReport(), TimeSpan.FromSeconds(delay));
    }

    private void PublishCountReport()
    {
        if (_state.IsAcked())
        {
            _log.Info("CountReportProtocol {0} incrementing sequence number", _state.Reporter);
            _state = _state.IncrementSequenceNumber();
        }
        PublishCurrentCountReport();
    }

    private bool PublishCurrentCountReport()
    {
        if (_state.IsAcked()) return false;
        var template = new CountReport
        {
            Reporter = _state.Reporter,
            SequenceNumber = _state.SequenceNumber,
            Count = _state.PlantCount,
            Comment = ""
        };


        var viaProcessorPath = new CountReport(template) { Comment = "via processor path" };
        _log.Info("CountReportProtocol {0} publishing report {1}", _state.Reporter, viaProcessorPath);
        _mediator.Tell(new SendToAll(CountReport.ProcessorPath, viaProcessorPath));

        var viaTopic = new CountReport(template) { Comment = "via topic" };
        _log.Info("CountReportProtocol {0} publishing report {1}", _state.Reporter, viaTopic);
        _mediator.Tell(new Publish(CountReport.Topic, viaTopic));

        return true;
    }

    private record NextCountReport();

    public record ProtocolState(string Reporter, int SequenceNumber, int SequenceNumberAck, int PlantCount);
}
