namespace MemoryCore;

internal interface IDateTimeOffsetProvider
{
    public DateTimeOffset Now { get; }
}

internal sealed class DateTimeOffsetProvider : IDateTimeOffsetProvider
{
    public DateTimeOffsetProvider() { }
    public DateTimeOffsetProvider(TimeSpan fromNow) : this(DateTimeOffset.UtcNow + fromNow) { }
    public DateTimeOffsetProvider(DateTimeOffset now)
    {
        this.now = now;
    }

    private readonly DateTimeOffset? now;
    public DateTimeOffset Now =>
        now ?? DateTimeOffset.UtcNow;
}
