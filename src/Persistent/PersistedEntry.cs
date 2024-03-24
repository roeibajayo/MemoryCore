namespace MemoryCore.Persistent;

public class PersistedEntry
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public string Key { get; set; }
    public object Value { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public string[]? Tags { get; set; }
    public DateTimeOffset? AbsoluteExpiration { get; set; }
    public long? SlidingExpiration { get; set; }
}