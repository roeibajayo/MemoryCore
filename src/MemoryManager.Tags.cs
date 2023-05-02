namespace MemoryCore;

internal partial class MemoryManager : IMemoryCore
{
    public IEnumerable<string> GetTags()
    {
        return _entries.Values
            .SelectMany(x => x.Tags)
            .Distinct();
    }

    public bool ExistsTag(string tag)
    {
        return _entries.Values
            .Any(x => x.Tags.Contains(tag, _entries.Comparer));
    }

    public void RemoveTag(string tag)
    {
        var keys = _entries.Values
            .Where(x => x.Tags.Contains(tag, _entries.Comparer))
            .Select(x => x.Key);

        foreach (var key in keys)
            _entries.TryRemove(key, out _);
    }
}
