using MemoryCore.Interfaces;
using MemoryCore.Utils;

namespace MemoryCore;

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
