namespace MemoryCore;

internal sealed class MemoryEntry
{
    internal string Key { get; init; }
    internal object? Value { get; init; }
    internal string[] Tags { get; init; }
    internal DateTimeOffset? Date { get; set; }
    internal TimeSpan? SlidingExpiration { get; set; }
    internal DateTimeOffset? LastTouch { get; set; }

    internal bool IsExpired(DateTimeOffset? now = null)
    {
        now ??= DateTimeOffset.Now;

        return (Date is not null && Date < now) ||
            (SlidingExpiration is not null && LastTouch + SlidingExpiration < now);
    }

    internal bool IsTagged(string tag, IEqualityComparer<string> comparer)
    {
        return Tags.Contains(tag, comparer);
    }

    internal void Touch(DateTimeOffset? date = null)
    {
        if (SlidingExpiration is null)
            return;

        LastTouch = date ?? DateTimeOffset.Now;
    }
}