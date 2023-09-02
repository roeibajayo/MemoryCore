using MemoryCore.Utils;

namespace MemoryCore;

internal interface IDateTimeOffsetProvider
{
    public long Now { get; }
    public DateTimeOffset NowOffset { get; }
}

internal sealed class DateTimeOffsetProvider : IDateTimeOffsetProvider
{
    public DateTimeOffsetProvider() { }
    public DateTimeOffsetProvider(TimeSpan fromNow) { 
        now = DateTimeUtils.Now + (long)fromNow.TotalMilliseconds;
    }

    private readonly long? now;
    public long Now =>
        now ?? DateTimeUtils.Now;
    public DateTimeOffset NowOffset =>
        DateTimeOffset.UtcNow;
}
