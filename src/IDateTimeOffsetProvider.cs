namespace MemoryCore;

internal interface IDateTimeOffsetProvider
{
    public DateTimeOffset Now { get; }
}

internal sealed class DateTimeOffsetProvider : IDateTimeOffsetProvider
{
    public DateTimeOffsetProvider() : this(DateTimeOffset.UtcNow) { }
    public DateTimeOffsetProvider(TimeSpan fromNow) : this(DateTimeOffset.UtcNow + fromNow) { }
    public DateTimeOffsetProvider(DateTimeOffset now)
    {
        Now = now;
    }

    public DateTimeOffset Now { get; }
}
