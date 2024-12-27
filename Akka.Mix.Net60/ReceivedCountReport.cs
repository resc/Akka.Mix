namespace Akka.Mix.Net60;

public record ReceivedCountReport(long Timestamp, string Reporter, int SequenceNumber, int Count);
