namespace MemoryCore;

public partial class MemoryCoreManager : IMemoryCore
{
    /// <summary>
    /// Get all keys in the cache by tag name.
    /// </summary>
    public IEnumerable<string> GetKeys(string tag)
    {
        if (entries.Values.Count == 0)
            return [];

        return entries.Values
            .Where(x => x.IsTagged(tag, comparer))
            .Select(x => x.Key)
            .Distinct();
    }

    /// <summary>
    /// Get all tags in the cache.
    /// </summary>
    public IEnumerable<string> GetTags()
    {
        if (entries.Values.Count == 0)
            return [];

        return entries.Values
            .Where(x => x.Tags is not null)
            .SelectMany(x => x.Tags!)
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
        if (this.entries.Values.Count == 0)
            return;

        var entries = this.entries.Values
            .Where(x => x.IsTagged(tag, comparer))
            .ToArray();

        if (entries.Length == 0)
            return;

        foreach (var entry in entries)
            this.entries.TryRemove(entry.Key, out _);

        persistedStore.Delete(Name, comparer, entries.Where(x => x.Persist).Select(x => x.Key));
    }
}
