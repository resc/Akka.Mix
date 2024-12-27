using System.Diagnostics.CodeAnalysis;

namespace Akka.Mix.Net60;

public class ReceivedCountReportComparer : IComparer<ReceivedCountReport>, IEqualityComparer<ReceivedCountReport>
{
    public static ReceivedCountReportComparer Default { get; } = new();

    private ReceivedCountReportComparer() { }

    public int Compare(ReceivedCountReport? x, ReceivedCountReport? y)
    {
        if (x == null) { return y == null ? 0 : -1; }
        if (y == null) { return 1; }
        var result = string.CompareOrdinal(x.Reporter, y.Reporter);
        if (result != 0) return result;
        return x.SequenceNumber.CompareTo(y.SequenceNumber);
    }

    public bool Equals(ReceivedCountReport? x, ReceivedCountReport? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x == null || y == null) return false;
        return x.Reporter == y.Reporter && x.SequenceNumber == y.SequenceNumber;
    }

    public int GetHashCode([DisallowNull] ReceivedCountReport obj)
    {
        if (obj == null) return 0;
        return HashCode.Combine(obj.Reporter, obj.SequenceNumber);
    }
}