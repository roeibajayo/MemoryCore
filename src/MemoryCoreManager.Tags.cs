namespace MemoryCore;

internal partial class MemoryCoreManager : IMemoryCore
{
    public IEnumerable<string> GetTags()
    {
        return entries.Values
            .SelectMany(x => x.Tags)
            .Distinct()
            .ToArray();
    }

    public bool ExistsTag(string tag)
    {
        return entries.Values
            .Any(x => x.IsTagged(tag, entries.Comparer));
    }

    public void RemoveTag(string tag)
    {
        var keys = entries.Values
            .Where(x => x.Tags.Contains(tag, entries.Comparer))
            .Select(x => x.Key)
            .ToArray();

        foreach (var key in keys)
            entries.TryRemove(key, out _);
    }
}
