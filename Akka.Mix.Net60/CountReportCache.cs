using System.Collections.Concurrent;
using Akka.Mix.Messages;

namespace Akka.Mix.Net60;

public class CountReportCache : ConcurrentDictionary<string, SortedSet<ReceivedCountReport>>
{
    private readonly int _maxReportsPerReporter;
    private readonly Func<long> _getTimestamp;

    // default constructor
    public CountReportCache() : this(10, () => System.Diagnostics.Stopwatch.GetTimestamp())
    {
    }

    public CountReportCache(int maxReportsPerReporter, Func<long> getTimestamp)
    {
        // check if maxReportsPerReporter is greater than 0, and throw an argument out of range exception if not
        if (maxReportsPerReporter <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxReportsPerReporter), maxReportsPerReporter, "value must be greater than 0");
        }
        _maxReportsPerReporter = maxReportsPerReporter;
        _getTimestamp = getTimestamp ?? throw new ArgumentNullException(nameof(getTimestamp));
    }
    public bool TryAdd(CountReport report)
    {
        var receivedReport = new ReceivedCountReport(_getTimestamp(), report.Reporter, report.SequenceNumber, report.Count);
        var set = GetOrAdd(report.Reporter, _ => new SortedSet<ReceivedCountReport>(ReceivedCountReportComparer.Default));
        var result = set.Add(receivedReport);

        DiscardOldItems(set);
        return result;
    }

    private void DiscardOldItems(SortedSet<ReceivedCountReport> set)
    {
        var discardCount = set.Count - _maxReportsPerReporter;
        if (discardCount > 0)
        {
            var toDiscard = set.OrderBy(r => r.Timestamp).Take(discardCount).ToList();
            foreach (var item in toDiscard)
            {
                set.Remove(item);
            }
        }
    }
}
