namespace MemoryCore.Persistent;

public class PersistedEntry
{
    public string Key { get; set; }
    public object Value { get; set; }
    public string[]? Tags { get; set; }
    public DateTimeOffset? AbsoluteExpiration { get; set; }
    public long? SlidingExpiration { get; set; }
}