namespace Akka.Mix.Leader;

public record ReceivedCountReport(long Timestamp, string Reporter, int SequenceNumber, int Count);
