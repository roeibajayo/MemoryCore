namespace MemoryCore.Interfaces;

internal interface IDateTimeOffsetProvider
{
    public long Now { get; }
    public DateTimeOffset NowOffset { get; }
}
