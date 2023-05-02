namespace MemoryCore;

internal sealed class MemoryEntry
{
    internal string Key { get; init; }
    internal object? Value { get; init; }
    internal string[] Tags { get; init; }
    internal DateTimeOffset? Date { get; set; }
    internal TimeSpan? SlidingExpiration { get; set; }
    internal DateTimeOffset Touch { get; set; }
}