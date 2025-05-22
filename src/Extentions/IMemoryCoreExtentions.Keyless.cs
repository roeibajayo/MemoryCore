namespace MemoryCore;

public static class IMemoryCoreExtensionsSecured
{
    private const string SECURED_PREFIX_KEY = "__MEMORYCORE_";

    /// <summary>
    /// Adds an keyless item to the cache with absolute expiration.
    /// </summary>
    /// <returns>Key of the item.</returns>
    public static Guid Add<T>(this IMemoryCore cache, T value, TimeSpan absoluteExpiration, params string[] tags)
    {
        var key = Guid.NewGuid();
        cache.Add(SECURED_PREFIX_KEY + key, value!, absoluteExpiration, tags);
        return key;
    }

    /// <summary>
    /// Adds an keyless item to the cache with absolute expiration.
    /// </summary>
    /// <returns>Key of the item.</returns>
    public static Guid Add<T>(this IMemoryCore cache, T value, int minutes, params string[] tags) =>
        cache.Add(value, TimeSpan.FromMinutes(minutes), tags);

    /// <summary>
    /// Adds an keyless item to the cache with sliding expiration.
    /// </summary>
    /// <returns>Key of the item.</returns>
    public static Guid AddSliding<T>(this IMemoryCore cache, T value, TimeSpan slidingExpiration,
        TimeSpan? absoluteExpiration = null, params string[] tags)
    {
        var key = Guid.NewGuid();
        cache.AddSliding(SECURED_PREFIX_KEY + key, value!, slidingExpiration, absoluteExpiration, tags);
        return key;
    }

    /// <summary>
    /// Adds an keyless item to the cache with sliding expiration.
    /// </summary>
    /// <returns>Key of the item.</returns>
    public static Guid AddSliding<T>(this IMemoryCore cache, T value, int minutes,
        TimeSpan? absoluteExpiration = null, params string[] tags) =>
        cache.AddSliding(value, TimeSpan.FromMinutes(minutes), absoluteExpiration, tags);

    /// <summary>
    /// Check if an item exists in the cache.
    /// </summary>
    /// <returns>True if the item exists, otherwise False.</returns>
    public static bool Exists(this IMemoryCore cache, Guid key) =>
        cache.Exists(SECURED_PREFIX_KEY + key);

    /// <summary>
    /// Gets an item from the cache.
    /// </summary>
    /// <returns>True if the item exists, otherwise False.</returns>
    public static bool TryGet<T>(this IMemoryCore cache, Guid key, out T? item)
    {
        if (cache.TryGet(SECURED_PREFIX_KEY + key, out var outItem))
        {
            item = (T?)outItem;
            return true;
        }

        item = default;
        return false;
    }

    /// <summary>
    /// Remove an item from the cache.
    /// </summary>
    public static void Remove(this IMemoryCore cache, Guid key) =>
        cache.Remove(SECURED_PREFIX_KEY + key);

    /// <summary>
    /// Remove all keyless items from the cache.
    /// </summary>
    public static void CleanNonKeyed(this IMemoryCore cache) =>
        cache.RemoveByPrefix(SECURED_PREFIX_KEY);

}
