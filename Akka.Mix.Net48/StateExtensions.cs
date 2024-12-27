using Akka.Mix.Messages;

namespace Akka.Mix.Net48;

public static class StateExtensions
{
    public static bool IsAcked(this CountReporter.ProtocolState state)
        => state.SequenceNumberAck == state.SequenceNumber;

    public static CountReporter.ProtocolState IncrementSequenceNumber(this CountReporter.ProtocolState state)
        => state with { SequenceNumber = state.SequenceNumber + 1 };

    public static bool CanBeAckedBy(this CountReporter.ProtocolState state, AckCountReport message)
        => state.Reporter == message.Reporter && state.SequenceNumber == message.SequenceNumber;

    public static bool CanBeNackedBy(this CountReporter.ProtocolState state, NackCountReport message)
        => state.Reporter == message.Reporter && state.SequenceNumber == message.SequenceNumber;

    public static CountReporter.ProtocolState AckSequenceNumber(this CountReporter.ProtocolState state, AckCountReport ack)
    {
        if (!state.CanBeAckedBy(ack))
            throw new InvalidOperationException("PlantReporter or sequence number mismatch for ack");

        if (state.IsAcked())
        {
            return state;
        }
        return state with { SequenceNumberAck = ack.SequenceNumber };
    }
}
