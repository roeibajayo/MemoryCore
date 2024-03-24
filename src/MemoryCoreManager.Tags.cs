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

        var now = dateTimeOffsetProvider.Now;
        return entries.Values
            .Where(x => !x.IsExpired(now) && x.IsTagged(tag, comparer))
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

        var now = dateTimeOffsetProvider.Now;
        return entries.Values
            .Where(x => !x.IsExpired(now) && x.Tags is not null)
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

        var now = dateTimeOffsetProvider.Now;
        return entries.Values
            .Any(x => !x.IsExpired(now) && x.IsTagged(tag, comparer));
    }

    /// <summary>
    /// Remove all items with a specific tag from the cache.
    /// </summary>
    public void RemoveTag(string tag)
    {
        if (this.entries.Values.Count == 0)
            return;

        var now = dateTimeOffsetProvider.Now;
        var entries = this.entries.Values
            .Where(x => !x.IsExpired(now) && x.IsTagged(tag, comparer));

        var deletePersistedKeys = new List<string>();
        foreach (var entry in entries)
        {
            this.entries.TryRemove(entry.Key, out _);

            if (entry.Persist)
                deletePersistedKeys.Add(entry.Key);
        }

        if (deletePersistedKeys.Count == 0)
            return;

        persistedStore.Delete(Name, comparer, deletePersistedKeys);
    }
}
