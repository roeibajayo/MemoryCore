namespace MemoryCore;

public partial class MemoryCoreManager : IMemoryCore
{
    /// <summary>
    /// Get all tags in the cache.
    /// </summary>
    public IEnumerable<string> GetTags()
    {
        if (entries.Values.Count == 0)
            return Enumerable.Empty<string>();

        return entries.Values
            .Where(x => x.Tags is not null)
            .SelectMany(x => x.Tags)
            .Distinct();
    }

    /// <summary>
    /// Check if a tag exists in the cache.
    /// </summary>
    /// <param name="tag"></param>
    /// <returns>True if the tag exists, false otherwise.</returns>
    public bool ExistsTag(string tag)
    {
        if (entries.Values.Count == 0)
            return false;

        return entries.Values
            .Any(x => x.IsTagged(tag, comparer));
    }

    /// <summary>
    /// Remove all items with a specific tag from the cache.
    /// </summary>
    public void RemoveTag(string tag)
    {
        if (entries.Values.Count == 0)
            return;

        var keys = entries.Values
            .Where(x => x.Tags?.Any(x => x?.Equals(tag, comparer) ?? false) ?? false)
            .Select(x => x.Key);

        foreach (var key in keys)
            entries.TryRemove(key, out _);
    }
}
