using Microsoft.Extensions.Caching.Memory;

namespace MemoryCore;

public interface IMemoryCore : IMemoryCache
{
    /// <summary>
    /// Add a new item to the cache.
    /// </summary>
    void Add(string key, object value, TimeSpan absoluteExpiration, params string[] tags);

    /// <summary>
    /// Add a new item to the cache with a sliding expiration.
    /// </summary>
    void AddSliding(string key, object value, TimeSpan slidingExpiration, TimeSpan? absoluteExpiration = null, params string[] tags);

    /// <summary>
    /// Check if an item exists in the cache.
    /// </summary>
    /// <returns>True if the item exists, false otherwise.</returns>
    bool Exists(string key);

    /// <summary>
    /// Check if a tag exists in the cache.
    /// </summary>
    /// <param name="tag"></param>
    /// <returns>True if the tag exists, false otherwise.</returns>
    bool ExistsTag(string tag);

    /// <summary>
    /// Get all keys in the cache.
    /// </summary>
    IEnumerable<string> GetKeys();

    /// <summary>
    /// Get all tags in the cache.
    /// </summary>
    IEnumerable<string> GetTags();

    /// <summary>
    /// Remove an item from the cache.
    /// </summary>
    void Remove(string key);

    /// <summary>
    /// Remove all items with a specific tag from the cache.
    /// </summary>
    void RemoveTag(string tag);

    /// <summary>
    /// Try to get an item from the cache.
    /// </summary>
    /// <returns>True if the item exists, false otherwise.</returns>
    bool TryGet(string key, out object? item);

    /// <summary>
    /// Try to get an item from the cache, or set it if it doesn't exist.
    /// </summary>
    /// <returns>The item from the cache, or the result of the function.</returns>
    T? TryGetOrAdd<T>(string key, Func<T> getValueFunction, TimeSpan absoluteExpiration,
        bool allowDefault = false, bool forceSet = false, params string[] tags);

    /// <summary>
    /// Try to get an item from the cache, or set it if it doesn't exist.
    /// </summary>
    /// <returns>The item from the cache, or the result of the function.</returns>
    Task<T?> TryGetOrAddAsync<T>(string key, Func<Task<T>> getValueFunction, TimeSpan absoluteExpiration,
        bool allowDefault = false, bool forceSet = false, params string[] tags);

    /// <summary>
    /// Try to get an item from the cache, or set it if it doesn't exist.
    /// </summary>
    /// <returns>The item from the cache, or the result of the function.</returns>
    T? TryGetOrAddSliding<T>(string key, Func<T> getValueFunction, TimeSpan slidingExpiration, TimeSpan? absoluteExpiration = null,
        bool allowDefault = false, bool forceSet = false, params string[] tags);

    /// <summary>
    /// Try to get an item from the cache, or set it if it doesn't exist.
    /// </summary>
    /// <returns>The item from the cache, or the result of the function.</returns>
    Task<T?> TryGetOrAddSlidingAsync<T>(string key, Func<Task<T>> getValueFunction, TimeSpan slidingExpiration, TimeSpan? absoluteExpiration = null,
        bool allowDefault = false, bool forceSet = false, params string[] tags);

    /// <summary>
    /// Get the number of items in the cache.
    /// </summary>
    int Count();

    /// <summary>
    /// Clear the cache.
    /// </summary>
    void Clear();
}